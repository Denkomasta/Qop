import { useState } from 'react'
import { createFileRoute, useNavigate } from '@tanstack/react-router'
import { useTranslation } from 'react-i18next'
import { CheckCircle2 } from 'lucide-react'
import { AsyncButton } from '@/components/ui/Button'
import { Spinner } from '@/components/ui/Spinner'
import { useAuthStore } from '@/store/useAuthStore'

import {
  QuestionCard,
  type QuizQuestionDto,
} from '@/components/quizzes/QuestionCard'
import { QuizStartScreen } from './-/QuizStartScreen'
import { QuestionTransitionScreen } from './-/QuizTransitionScreen'
import { QuestionRecapScreen } from './-/QuestionRecapScreen'
import { QuizRecapScreen } from './-/QuizRecapScreen'

// Import your actual Orval hooks here
// import { useGetApiQuizzesQuizId } from '@/api/generated/endpoints/quizzes/quizzes'
// import { usePostApiQuizAttemptsStart, usePostApiQuizAttemptsIdAnswer, usePostApiQuizAttemptsIdComplete } from '@/api/generated/endpoints/quiz-attempts/quiz-attempts'

export const Route = createFileRoute(
  '/app/_authenticated/quizzes/$quizId/play/',
)({
  component: QuizTakePage,
})

type QuizPhase = 'start' | 'transition' | 'answering' | 'recap' | 'completed'

function QuizTakePage() {
  const { t } = useTranslation()
  const { quizId } = Route.useParams()
  const navigate = useNavigate()
  const { user } = useAuthStore()

  const [phase, setPhase] = useState<QuizPhase>('start')
  const [attemptId, setAttemptId] = useState<number | null>(null)
  const [currentIndex, setCurrentIndex] = useState(0)

  const [questionStartTime, setQuestionStartTime] = useState<number>(Date.now())
  const [correctAnswersCount, setCorrectAnswersCount] = useState(0)

  const [answers, setAnswers] = useState<
    Record<
      number,
      { selectedOptionIds: (number | string)[]; timeSpentMs: number }
    >
  >({})
  const [currentCorrectOptionIds, setCurrentCorrectOptionIds] = useState<
    (number | string)[]
  >([])

  // --- MOCKED API DATA (Replace with your actual hooks) ---
  // const { data: quiz } = useGetApiQuizzesQuizId(Number(quizId))
  const isLoading = false
  const quiz = {
    id: Number(quizId),
    title: 'Introduction to Computer Science',
    quizQuestions: [
      {
        id: 101,
        text: 'What does CPU stand for?',
        options: [
          { id: 1, text: 'Central Process Unit' },
          { id: 2, text: 'Computer Personal Unit' },
          { id: 3, text: 'Central Processing Unit' },
          { id: 4, text: 'Central Processor Unit' },
        ],
      },
      {
        id: 102,
        text: 'Which of the following is not a programming language?',
        options: [
          { id: 5, text: 'Python' },
          { id: 6, text: 'HTML' },
          { id: 7, text: 'Java' },
          { id: 8, text: 'C++' },
        ],
      },
    ] as QuizQuestionDto[],
  }

  const questions = quiz?.quizQuestions || []
  const totalQuestions = questions.length
  const currentQuestion = questions[currentIndex]

  const handleAttemptStarted = (newAttemptId: number) => {
    setAttemptId(newAttemptId)
    setPhase('transition')
  }

  const handleTransitionComplete = () => {
    setPhase('answering')
    setQuestionStartTime(Date.now())
  }

  const handleOptionSelect = (
    questionId: number | string,
    optionId: number | string,
  ) => {
    const timeSpentSoFar = Date.now() - questionStartTime

    setAnswers((prev) => {
      const existing = prev[Number(questionId)]

      const newSelection = [optionId]

      return {
        ...prev,
        [Number(questionId)]: {
          selectedOptionIds: newSelection,
          timeSpentMs: (existing?.timeSpentMs || 0) + timeSpentSoFar,
        },
      }
    })

    setQuestionStartTime(Date.now())
  }

  const handleAnswerSubmit = async () => {
    if (!attemptId || !currentQuestion) return

    const currentAnswer = answers[Number(currentQuestion.id)]
    if (!currentAnswer || currentAnswer.selectedOptionIds.length === 0) {
      alert(
        t(
          'quiz.selectAnswerWarning',
          'Please select an answer before continuing.',
        ),
      )
      return
    }

    try {
      const finalTimeSpent =
        currentAnswer.timeSpentMs + (Date.now() - questionStartTime)

      // 2. Fire your actual API endpoint to save this specific answer
      /*
      const response = await answerMutation.mutateAsync({
        id: attemptId,
        data: {
          quizQuestionId: Number(currentQuestion.id),
          responseTimeMs: finalTimeSpent,
          freeTextAnswer: null,
          selectedOptionIds: currentAnswer.selectedOptionIds
        }
      })
      // Extract the correct answers from your backend response!
      const correctIdsFromApi = response.correctOptionIds 
      */

      await new Promise((r) => setTimeout(r, 800))
      const correctIdsFromApi = currentIndex === 0 ? [3] : [6]

      setCurrentCorrectOptionIds(correctIdsFromApi)

      const isFullyCorrect =
        currentAnswer.selectedOptionIds.length === correctIdsFromApi.length &&
        currentAnswer.selectedOptionIds.every((id) =>
          correctIdsFromApi.includes(id),
        )

      if (isFullyCorrect) {
        setCorrectAnswersCount((prev) => prev + 1)
      }

      setPhase('recap')
    } catch (error) {
      console.error('Failed to submit answer', error)
      alert(t('common.error', 'An error occurred. Please try again.'))
    }
  }

  const handleRecapContinue = async () => {
    if (currentIndex < totalQuestions - 1) {
      setCurrentIndex((prev) => prev + 1)
      setPhase('transition')
    } else {
      try {
        // await completeMutation.mutateAsync({ id: attemptId! })
        await new Promise((r) => setTimeout(r, 1000)) // Mock delay

        setPhase('completed')
      } catch (error) {
        console.error('Failed to complete quiz', error)
      }
    }
  }

  if (isLoading || !quiz) {
    return (
      <div className="flex min-h-[80vh] flex-col items-center justify-center gap-4">
        <Spinner size="lg" />
      </div>
    )
  }

  if (phase === 'start') {
    return (
      <QuizStartScreen
        quizId={quiz.id}
        quizTitle={quiz.title}
        enrollmentId={123}
        onAttemptStarted={handleAttemptStarted}
        onCancel={() => history.back()}
      />
    )
  }

  if (phase === 'transition') {
    return (
      <QuestionTransitionScreen
        questionNumber={currentIndex + 1}
        totalQuestions={totalQuestions}
        onComplete={handleTransitionComplete}
      />
    )
  }

  if (phase === 'answering' && currentQuestion) {
    const currentAnswer = answers[Number(currentQuestion.id)]
    const hasSelected =
      currentAnswer && currentAnswer.selectedOptionIds.length > 0

    return (
      <div className="mx-auto flex min-h-[calc(100vh-4rem)] max-w-3xl animate-in flex-col p-4 duration-500 fade-in md:p-6 lg:p-8">
        <div className="mb-6 space-y-2">
          <div className="flex justify-between text-sm font-medium text-muted-foreground">
            <span>
              {t('quiz.questionProgress', {
                current: currentIndex + 1,
                total: totalQuestions,
              })}
            </span>
          </div>
          <div className="h-2.5 w-full overflow-hidden rounded-full bg-secondary">
            <div
              className="h-full bg-primary transition-all duration-300 ease-in-out"
              style={{ width: `${(currentIndex / totalQuestions) * 100}%` }}
            />
          </div>
        </div>

        <QuestionCard
          question={currentQuestion}
          selectedOptionIds={currentAnswer?.selectedOptionIds || []}
          onSelectOption={handleOptionSelect}
        />

        <div className="mt-8 flex justify-end">
          <AsyncButton
            size="lg"
            onClick={handleAnswerSubmit}
            disabled={!hasSelected}
            className="w-full shadow-md sm:w-auto"
            loadingText={t('quiz.submitAnswer')}
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
        selectedOptionIds={
          answers[Number(currentQuestion.id)]?.selectedOptionIds || []
        }
        correctOptionIds={currentCorrectOptionIds}
        onContinue={handleRecapContinue}
        isLastQuestion={currentIndex === totalQuestions - 1}
      />
    )
  }

  if (phase === 'completed') {
    return (
      <QuizRecapScreen
        quizId={quiz.id}
        quizTitle={quiz.title}
        totalQuestions={totalQuestions}
        correctCount={correctAnswersCount}
      />
    )
  }

  return null
}
