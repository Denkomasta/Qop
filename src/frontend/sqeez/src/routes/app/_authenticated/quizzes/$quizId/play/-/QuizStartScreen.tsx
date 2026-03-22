import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { AlertTriangle, PlayCircle, Rocket } from 'lucide-react'
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from '@/components/ui/Card'
import { AsyncButton, Button } from '@/components/ui/Button'
import { usePostApiQuizAttemptsStart } from '@/api/generated/endpoints/quiz-attempts/quiz-attempts'

interface QuizStartScreenProps {
  quizId: number
  quizTitle: string
  enrollmentId: number
  onAttemptStarted: (
    newAttemptId: number,
    firstQuestionId: number | null,
  ) => void
  onCancel: () => void
}

export function QuizStartScreen({
  quizId,
  quizTitle,
  enrollmentId,
  onAttemptStarted,
  onCancel,
}: QuizStartScreenProps) {
  const { t } = useTranslation()
  const [isStarting, setIsStarting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const startMutation = usePostApiQuizAttemptsStart()

  const handleStartAttempt = async () => {
    setIsStarting(true)
    setError(null)

    try {
      const response = await startMutation.mutateAsync({
        data: {
          quizId: quizId,
          enrollmentId: enrollmentId,
        },
      })
      onAttemptStarted(
        Number(response.id),
        response?.nextQuestionId ? Number(response.nextQuestionId) : null,
      )
    } catch (err) {
      console.error('Failed to start quiz attempt:', err)
      setError(
        t('quiz.startError', 'Failed to start the quiz. Please try again.'),
      )
    } finally {
      setIsStarting(false)
    }
  }

  return (
    <div className="flex min-h-[60vh] items-center justify-center p-4">
      <Card className="w-full max-w-lg border-primary/20 shadow-lg">
        <CardHeader className="pb-6 text-center">
          <div className="mx-auto mb-4 flex h-20 w-20 items-center justify-center rounded-full bg-primary/10">
            <Rocket className="h-10 w-10 text-primary" />
          </div>
          <CardTitle className="text-2xl font-bold">
            {t('quiz.readyToStart', 'Are you ready?')}
          </CardTitle>
          <CardDescription className="mt-2 text-base">
            {quizTitle}
          </CardDescription>
        </CardHeader>

        <CardContent className="space-y-4 pb-6 text-center">
          <div className="rounded-lg bg-amber-500/10 p-4 text-amber-600 dark:text-amber-500">
            <div className="flex items-start gap-3 text-left text-sm font-medium">
              <AlertTriangle className="h-5 w-5 shrink-0" />
              <p>
                {t(
                  'quiz.startWarning',
                  'Clicking start will immediately consume one of your attempts. Make sure you have a stable connection and enough time to complete it.',
                )}
              </p>
            </div>
          </div>

          {error && (
            <p className="text-sm font-medium text-destructive">{error}</p>
          )}
        </CardContent>

        <CardFooter className="flex flex-col-reverse gap-3 sm:flex-row sm:justify-end">
          <Button
            variant="ghost"
            onClick={onCancel}
            disabled={isStarting}
            className="w-full sm:w-fit"
          >
            {t('common.cancel', 'Cancel')}
          </Button>
          <AsyncButton
            size="lg"
            onClick={handleStartAttempt}
            disabled={isStarting}
            className="w-full sm:w-fit"
            loadingText={t('quiz.beginAttempt')}
          >
            <PlayCircle className="mr-2 size-5" />
            {t('quiz.beginAttempt')}
          </AsyncButton>
        </CardFooter>
      </Card>
    </div>
  )
}
