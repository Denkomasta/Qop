import { useEffect } from 'react'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { useAuthStore } from '@/store/useAuthStore'

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
import { useQuizStore } from '@/store/useQuizStore'

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

  const state = useQuizStore()
  const { actions } = state

  console.log(state.phase)

  useEffect(() => {
    if (initialAttemptId && state.attemptId !== initialAttemptId) {
      actions.initResume(initialAttemptId)
    }
  }, [initialAttemptId, actions, state.attemptId])

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
      Number(state.currentQuestionId),
      {
        query: {
          enabled:
            !!state.currentQuestionId &&
            // prefetches the question
            (state.phase === 'transition' || state.phase === 'answering'),
          refetchOnWindowFocus: false,
          staleTime: 1000 * 60,
        },
      },
    )

  useEffect(() => {
    // If we opened a different quiz, wipe the store
    if (state.activeQuizId !== null && state.activeQuizId !== quizId) {
      actions.resetQuiz()
    }
    // Lock in the current quiz ID so we remember it for next time
    else if (state.activeQuizId !== quizId) {
      useQuizStore.setState({ activeQuizId: quizId })
    }
  }, [quizId, state.activeQuizId, actions])

  useEffect(() => {
    if (initialAttemptId && state.attemptId !== initialAttemptId) {
      actions.initResume(initialAttemptId)
    }
  }, [initialAttemptId, actions, state.attemptId])

  useEffect(() => {
    if (currentQuestion && state.phase === 'transition') {
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
  }, [currentQuestion, state.phase])

  const {
    data: nextPendingQuestionId,
    isSuccess: isResumingSuccess,
    isError: isResumingError,
  } = useGetApiQuizAttemptsIdNextQuestion(Number(state.attemptId), {
    query: {
      enabled: state.phase === 'resuming' && !!state.attemptId,
      refetchOnWindowFocus: false,
    },
  })

  const answerMutation = usePostApiQuizAttemptsIdAnswer()
  const completeMutation = usePostApiQuizAttemptsIdComplete()

  useEffect(() => {
    if (state.phase === 'resuming') {
      if (isResumingError) {
        toast.error(t('quiz.resumeError'))
        history.back()
        return
      }

      if (isResumingSuccess) {
        if (nextPendingQuestionId) {
          actions.startAttempt(state.attemptId!, Number(nextPendingQuestionId))
        } else {
          if (state.attemptId) {
            completeMutation
              .mutateAsync({ id: Number(state.attemptId) })
              .then((response) => {
                actions.completeQuiz(
                  !response.earnedBadges ? undefined : response.earnedBadges,
                )
              })
              .catch((error) => {
                console.error('Failed to auto-complete attempt', error)
                actions.completeQuiz()
              })
          } else {
            actions.completeQuiz()
          }
        }
      }
    }
  }, [
    state.phase,
    isResumingSuccess,
    isResumingError,
    nextPendingQuestionId,
    state.attemptId,
    completeMutation,
    actions,
    t,
  ])

  const handleAnswerSubmit = async () => {
    if (!state.attemptId || !state.currentQuestionId) return

    const hasSelection =
      state.selectedOptionIds.length > 0 ||
      state.freeTextValue.trim().length > 0

    if (!hasSelection) {
      toast.warning(t('quiz.selectAnswerWarning'))
      return
    }

    try {
      const timeSpentMs = Date.now() - state.questionStartTime
      const response = await answerMutation.mutateAsync({
        id: state.attemptId,
        data: {
          quizQuestionId: state.currentQuestionId,
          responseTimeMs: timeSpentMs,
          freeTextAnswer: state.freeTextValue || null,
          selectedOptionIds: state.selectedOptionIds,
        },
      })

      if (!response.correctOptionIds)
        throw new Error('Response did not return correct answers!')

      const correctIds = response.correctOptionIds

      const isFullyCorrect =
        state.selectedOptionIds.length === correctIds.length &&
        state.selectedOptionIds.every((id) => correctIds.includes(id))

      actions.submitAnswer({
        correctIds,
        nextQuestionId: response.nextQuestionId
          ? Number(response.nextQuestionId)
          : null,
        correctFreeTextAnswer: response.correctFreeTextAnswer || null,
        responseTimeMs: Number(response.responseTimeMs),
        isFullyCorrect,
      })
    } catch (error) {
      console.error('Failed to submit answer', error)
      toast.error(t('common.error'))
    }
  }

  const handleRecapContinue = async () => {
    if (state.nextQuestionId !== null) {
      actions.continueToNext()
    } else {
      try {
        if (state.attemptId) {
          const response = await completeMutation.mutateAsync({
            id: state.attemptId,
          })
          actions.completeQuiz(
            !response.earnedBadges ? undefined : response.earnedBadges,
          )
        } else {
          actions.completeQuiz()
        }
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
      ...state,
      isBootingUp,
      isQuestionLoading,
      quizData,
      userEnrollment: enrollmentData?.data?.[0],
      currentQuestion,
      totalQuestions: Number(quizData?.quizQuestions || 0),
      hasSelection:
        state.selectedOptionIds.length > 0 ||
        state.freeTextValue.trim().length > 0,
    },
    actions: {
      handleAttemptStarted: (
        attemptId: number,
        firstQuestionId: number | null,
      ) => actions.startAttempt(attemptId, firstQuestionId),
      handleTransitionComplete: () => actions.finishTransition(),
      resetEngine: () => actions.resetQuiz(),
      handleOptionSelect: (optId: number | string) =>
        actions.selectOption(optId),
      handleFreeTextChange: (text: string) => actions.setFreeText(text),
      handleAnswerSubmit,
      handleRecapContinue,
    },
  }
}
