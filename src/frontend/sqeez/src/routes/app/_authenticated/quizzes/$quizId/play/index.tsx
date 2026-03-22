import { useState } from 'react'
import { createFileRoute } from '@tanstack/react-router'
import { useTranslation } from 'react-i18next'
import { CheckCircle2 } from 'lucide-react'
import { AsyncButton } from '@/components/ui/Button'
import { Spinner } from '@/components/ui/Spinner'
import { useAuthStore } from '@/store/useAuthStore'

import { QuestionCard } from '@/components/quizzes/QuestionCard'
import { QuizStartScreen } from './-/QuizStartScreen'
import { QuestionRecapScreen } from './-/QuestionRecapScreen'
import { QuizRecapScreen } from './-/QuizRecapScreen'

import { toast } from 'sonner'
import { QuestionTransitionScreen } from './-/QuizTransitionScreen'
import {
  useGetApiQuizzesQuizId,
  useGetApiQuizzesQuizIdQuestionsQuestionIdDetailed,
} from '@/api/generated/endpoints/quizzes/quizzes'
import { useGetApiSubjectsSubjectIdEnrollments } from '@/api/generated/endpoints/subjects/subjects'
import {
  usePostApiQuizAttemptsIdAnswer,
  usePostApiQuizAttemptsIdComplete,
} from '@/api/generated/endpoints/quiz-attempts/quiz-attempts'

export const Route = createFileRoute(
  '/app/_authenticated/quizzes/$quizId/play/',
)({
  component: QuizTakePage,
})

type QuizPhase = 'start' | 'transition' | 'answering' | 'recap' | 'completed'

function QuizTakePage() {
  const { t } = useTranslation()
  const { quizId } = Route.useParams()
  const { user } = useAuthStore()
  const userId = user?.id

  const [phase, setPhase] = useState<QuizPhase>('start')
  const [attemptId, setAttemptId] = useState<number | null>(null)

  const [currentQuestionId, setCurrentQuestionId] = useState<number | null>(
    null,
  )
  const [nextQuestionId, setNextQuestionId] = useState<number | null>(null)

  const [questionStartTime, setQuestionStartTime] = useState<number>(() =>
    Date.now(),
  )
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

  const { data: currentQuestion, isLoading } =
    useGetApiQuizzesQuizIdQuestionsQuestionIdDetailed(
      Number(quizId),
      Number(currentQuestionId),
      {
        query: {
          enabled: !!currentQuestionId && phase === 'answering',
          refetchOnWindowFocus: false,
        },
      },
    )

  const answerMutation = usePostApiQuizAttemptsIdAnswer()
  const completeMutation = usePostApiQuizAttemptsIdComplete()

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

  const handleOptionSelect = (
    questionId: number | string,
    optionId: number | string,
  ) => {
    setSelectedOptionIds([optionId])
    setQuestionStartTime(Date.now())
  }

  const handleFreeTextChange = (questionId: number | string, text: string) => {
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

      const correctIds = response.correctOptionIds

      if (!correctIds) {
        throw new Error('Response did not return correct answers!')
      }

      setCurrentCorrectOptionIds(correctIds)

      setNextQuestionId(
        response.nextQuestionId ? Number(response.nextQuestionId) : null,
      )

      const isFullyCorrect =
        selectedOptionIds.length === correctIds.length &&
        selectedOptionIds.every((id) => correctIds.includes(id))

      if (isFullyCorrect) {
        setCorrectAnswersCount((prev) => prev + 1)
      }

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
        if (attemptId) {
          await completeMutation.mutateAsync({ id: attemptId })
        }
        setPhase('completed')
      } catch (error) {
        console.error('Failed to complete quiz', error)
        toast.error(t('common.error', 'Failed to finalize quiz.'))
      }
    }
  }

  if (
    isQuizLoading ||
    !quizData ||
    isEnrollmentLoading ||
    !enrollmentData?.data
  ) {
    return (
      <div className="flex min-h-[60vh] flex-col items-center justify-center gap-4">
        <Spinner size="lg" />
      </div>
    )
  }

  const totalQuestions = Number(quizData.quizQuestions)
  const userEnrollment = enrollmentData.data[0]

  if (phase === 'start') {
    return (
      <QuizStartScreen
        quizId={Number(quizId)}
        quizTitle={quizData.title}
        enrollmentId={Number(userEnrollment.id)}
        onAttemptStarted={handleAttemptStarted}
        onCancel={() => history.back()}
      />
    )
  }

  if (phase === 'transition') {
    return (
      <QuestionTransitionScreen
        questionNumber={questionsAnswered + 1}
        totalQuestions={totalQuestions}
        onComplete={handleTransitionComplete}
      />
    )
  }

  if (phase === 'answering' && currentQuestion) {
    if (isLoading) {
      return (
        <div className="flex min-h-[60vh] flex-col items-center justify-center gap-4">
          <Spinner size="lg" />
        </div>
      )
    }

    const hasSelection =
      selectedOptionIds.length > 0 || freeTextValue.trim().length > 0

    return (
      <div className="mx-auto flex min-h-[calc(100vh-4rem)] max-w-3xl animate-in flex-col p-4 duration-500 fade-in md:p-6 lg:p-8">
        <div className="mb-6 space-y-2">
          <div className="flex justify-between text-sm font-medium text-muted-foreground">
            <span>
              {t('quiz.questionProgress', {
                current: questionsAnswered + 1,
                total: totalQuestions,
              })}
            </span>
          </div>
          <div className="h-2.5 w-full overflow-hidden rounded-full bg-secondary">
            <div
              className="h-full bg-primary transition-all duration-300 ease-in-out"
              style={{
                width: `${(questionsAnswered / totalQuestions) * 100}%`,
              }}
            />
          </div>
        </div>

        <QuestionCard
          question={currentQuestion}
          selectedOptionIds={selectedOptionIds}
          onSelectOption={handleOptionSelect}
          freeTextValue={freeTextValue}
          onChangeFreeText={handleFreeTextChange}
        />

        <div className="mt-8 flex justify-end">
          <AsyncButton
            size="lg"
            onClick={handleAnswerSubmit}
            disabled={!hasSelection}
            className="w-full shadow-md sm:w-auto"
            loadingText={t('common.submitting', 'Submitting...')}
          >
            <CheckCircle2 className="mr-2 h-5 w-5" />
            {t('quiz.submitAnswer')}
          </AsyncButton>
        </div>
      </div>
    )
  }

  if (phase === 'recap' && currentQuestion) {
    return (
      <QuestionRecapScreen
        question={currentQuestion}
        selectedOptionIds={selectedOptionIds}
        correctOptionIds={currentCorrectOptionIds}
        onContinue={handleRecapContinue}
        isLastQuestion={nextQuestionId === null}
      />
    )
  }

  if (phase === 'completed') {
    return (
      <QuizRecapScreen
        quizId={quizId}
        quizTitle="Quiz Completed"
        totalQuestions={questionsAnswered}
        correctCount={correctAnswersCount}
      />
    )
  }

  return null
}
