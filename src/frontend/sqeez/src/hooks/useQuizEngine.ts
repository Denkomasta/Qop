import { useState, useEffect } from 'react'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { useAuthStore } from '@/store/useAuthStore'

// Import all your Orval hooks here
import {
  useGetApiQuizzesQuizId,
  useGetApiQuizzesQuizIdQuestionsQuestionIdDetailed,
} from '@/api/generated/endpoints/quizzes/quizzes'
import { useGetApiSubjectsSubjectIdEnrollments } from '@/api/generated/endpoints/subjects/subjects'
import {
  useGetApiQuizAttemptsIdNextQuestion,
  usePostApiQuizAttemptsIdAnswer,
  usePostApiQuizAttemptsIdComplete,
} from '@/api/generated/endpoints/quiz-attempts/quiz-attempts'
import { queryClient } from '@/main'
import {
  getApiMediaAssetsIdFile,
  getGetApiMediaAssetsIdFileQueryKey,
} from '@/api/generated/endpoints/media-assets/media-assets'

export type QuizPhase =
  | 'start'
  | 'resuming'
  | 'transition'
  | 'answering'
  | 'recap'
  | 'completed'

export function useQuizEngine(quizId: string, initialAttemptId?: number) {
  const { t } = useTranslation()
  const userId = useAuthStore((s) => s.user?.id)

  const [phase, setPhase] = useState<QuizPhase>(
    initialAttemptId ? 'resuming' : 'start',
  )
  const [attemptId, setAttemptId] = useState<number | null>(
    initialAttemptId || null,
  )
  const [currentQuestionId, setCurrentQuestionId] = useState<number | null>(
    null,
  )
  const [nextQuestionId, setNextQuestionId] = useState<number | null>(null)

  const [questionStartTime, setQuestionStartTime] = useState<number>(() =>
    Date.now(),
  )
  const [lastResponseTimeMs, setLastResponseTimeMs] = useState<number | null>(
    null,
  )
  const [correctFreeTextAnswer, setCorrectFreeTextAnswer] = useState<
    string | null
  >(null)
  const [correctAnswersCount, setCorrectAnswersCount] = useState(0)
  const [questionsAnswered, setQuestionsAnswered] = useState(0)

  const [selectedOptionIds, setSelectedOptionIds] = useState<
    (number | string)[]
  >([])
  const [freeTextValue, setFreeTextValue] = useState<string>('')
  const [currentCorrectOptionIds, setCurrentCorrectOptionIds] = useState<
    (number | string)[]
  >([])

  const { data: quizData, isLoading: isQuizLoading } = useGetApiQuizzesQuizId(
    quizId,
    { studentId: userId },
    { query: { enabled: !!userId } },
  )

  const subjectId = quizData?.subjectId

  const { data: enrollmentData, isLoading: isEnrollmentLoading } =
    useGetApiSubjectsSubjectIdEnrollments(
      subjectId!,
      { StudentId: userId },
      { query: { enabled: !!subjectId && !!userId } },
    )

  const { data: currentQuestion, isLoading: isQuestionLoading } =
    useGetApiQuizzesQuizIdQuestionsQuestionIdDetailed(
      Number(quizId),
      Number(currentQuestionId),
      {
        query: {
          enabled:
            !!currentQuestionId &&
            // prefetches the question
            (phase === 'transition' || phase === 'answering'),
          refetchOnWindowFocus: false,
          staleTime: 1000 * 60,
        },
      },
    )

  // prefetches the media for question
  useEffect(() => {
    if (currentQuestion && phase === 'transition') {
      const assetIdsToFetch: (number | string)[] = []

      if (currentQuestion.mediaAssetId) {
        assetIdsToFetch.push(currentQuestion.mediaAssetId)
      }

      currentQuestion.options.forEach((option) => {
        if (option.mediaAssetId) assetIdsToFetch.push(option.mediaAssetId)
      })

      assetIdsToFetch.forEach((id) => {
        const assetId = Number(id)

        queryClient.prefetchQuery({
          queryKey: getGetApiMediaAssetsIdFileQueryKey(assetId),
          queryFn: () => getApiMediaAssetsIdFile(assetId),
          staleTime: 1000 * 60 * 60,
        })
      })
    }
  }, [currentQuestion, phase])

  const {
    data: nextPendingQuestionId,
    isSuccess: isResumingSuccess,
    isError: isResumingError,
  } = useGetApiQuizAttemptsIdNextQuestion(Number(attemptId), {
    query: {
      enabled: phase === 'resuming' && !!attemptId,
      refetchOnWindowFocus: false,
    },
  })

  const answerMutation = usePostApiQuizAttemptsIdAnswer()
  const completeMutation = usePostApiQuizAttemptsIdComplete()

  useEffect(() => {
    if (phase === 'resuming') {
      if (isResumingError) {
        toast.error(t('quiz.resumeError'))
        history.back()
        return
      }

      if (isResumingSuccess) {
        if (nextPendingQuestionId) {
          setCurrentQuestionId(Number(nextPendingQuestionId))
          setPhase('transition')
        } else {
          setPhase('completed')
          if (attemptId) {
            completeMutation
              .mutateAsync({ id: Number(attemptId) })
              .catch((error) =>
                console.error('Failed to auto-complete attempt', error),
              )
          }
        }
      }
    }
  }, [
    phase,
    isResumingSuccess,
    isResumingError,
    nextPendingQuestionId,
    attemptId,
    completeMutation,
    t,
  ])

  const handleAttemptStarted = (
    newAttemptId: number,
    firstQuestionId: number | null,
  ) => {
    setAttemptId(newAttemptId)
    if (firstQuestionId) {
      setCurrentQuestionId(firstQuestionId)
      setPhase('transition')
    } else {
      setPhase('completed')
    }
  }

  const handleTransitionComplete = () => {
    setPhase('answering')
    setQuestionStartTime(Date.now())
    setSelectedOptionIds([])
    setFreeTextValue('')
    setCurrentCorrectOptionIds([])
  }

  const handleOptionSelect = (qId: number | string, optId: number | string) => {
    setSelectedOptionIds([optId])
    setQuestionStartTime(Date.now())
  }

  const handleFreeTextChange = (qId: number | string, text: string) => {
    setFreeTextValue(text)
  }

  const handleAnswerSubmit = async () => {
    if (!attemptId || !currentQuestionId) return

    const hasSelection =
      selectedOptionIds.length > 0 || freeTextValue.trim().length > 0
    if (!hasSelection) {
      toast.warning(
        t(
          'quiz.selectAnswerWarning',
          'Please select or type an answer before continuing.',
        ),
      )
      return
    }

    try {
      const timeSpentMs = Date.now() - questionStartTime
      const response = await answerMutation.mutateAsync({
        id: attemptId,
        data: {
          quizQuestionId: currentQuestionId,
          responseTimeMs: timeSpentMs,
          freeTextAnswer: freeTextValue || null,
          selectedOptionIds: selectedOptionIds,
        },
      })

      const correctIds = response.correctOptionIds || []

      setLastResponseTimeMs(Number(response.responseTimeMs))
      setCurrentCorrectOptionIds(correctIds)
      setCorrectFreeTextAnswer(response.freeTextAnswer || null)
      if (!correctIds)
        throw new Error('Response did not return correct answers!')

      setCurrentCorrectOptionIds(correctIds)
      setNextQuestionId(
        response.nextQuestionId ? Number(response.nextQuestionId) : null,
      )

      const isFullyCorrect =
        selectedOptionIds.length === correctIds.length &&
        selectedOptionIds.every((id) => correctIds.includes(id))

      if (isFullyCorrect) setCorrectAnswersCount((prev) => prev + 1)

      setQuestionsAnswered((prev) => prev + 1)
      setPhase('recap')
    } catch (error) {
      console.error('Failed to submit answer', error)
      toast.error(t('common.error'))
    }
  }

  const handleRecapContinue = async () => {
    if (nextQuestionId !== null) {
      setCurrentQuestionId(nextQuestionId)
      setPhase('transition')
    } else {
      try {
        if (attemptId) await completeMutation.mutateAsync({ id: attemptId })
        setPhase('completed')
      } catch (error) {
        console.error('Failed to complete quiz', error)
        toast.error(t('common.error'))
      }
    }
  }

  const isBootingUp =
    isQuizLoading || !quizData || isEnrollmentLoading || !enrollmentData?.data

  return {
    state: {
      phase,
      isBootingUp,
      isQuestionLoading,
      quizData,
      userEnrollment: enrollmentData?.data?.[0],
      currentQuestion,
      totalQuestions: Number(quizData?.quizQuestions || 0),
      questionsAnswered,
      correctAnswersCount,
      selectedOptionIds,
      freeTextValue,
      correctFreeTextAnswer,
      currentCorrectOptionIds,
      lastResponseTimeMs,
      nextQuestionId,
      hasSelection:
        selectedOptionIds.length > 0 || freeTextValue.trim().length > 0,
    },
    actions: {
      handleAttemptStarted,
      handleTransitionComplete,
      handleOptionSelect,
      handleFreeTextChange,
      handleAnswerSubmit,
      handleRecapContinue,
    },
  }
}
