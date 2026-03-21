import { useState, useEffect } from 'react'
import { useTranslation } from 'react-i18next'
import { Timer } from 'lucide-react'
import { Card, CardContent } from '@/components/ui/Card'

interface QuestionTransitionScreenProps {
  questionNumber: number
  totalQuestions: number
  countdownSeconds?: number
  onComplete: () => void
}

export function QuestionTransitionScreen({
  questionNumber,
  totalQuestions,
  countdownSeconds = 5,
  onComplete,
}: QuestionTransitionScreenProps) {
  const { t } = useTranslation()
  const [timeLeft, setTimeLeft] = useState(countdownSeconds)

  useEffect(() => {
    if (timeLeft <= 0) {
      onComplete()
      return
    }

    const timer = setInterval(() => {
      setTimeLeft((prev) => prev - 1)
    }, 1000)

    return () => clearInterval(timer)
  }, [timeLeft, onComplete])

  return (
    <div className="flex min-h-[50vh] animate-in flex-col items-center justify-center p-6 duration-300 fade-in zoom-in">
      <Card className="w-full max-w-sm border-primary/20 bg-primary/5 shadow-lg">
        <CardContent className="flex flex-col items-center justify-center p-10 text-center">
          <div className="mb-6 flex h-16 w-16 items-center justify-center rounded-full bg-primary/10">
            <Timer className="h-8 w-8 animate-pulse text-primary" />
          </div>

          <h2 className="mb-2 text-2xl font-bold tracking-tight text-foreground">
            {t('quiz.getReady', 'Get Ready!')}
          </h2>
          <p className="mb-8 font-medium text-muted-foreground">
            {t('quiz.questionProgress', {
              current: questionNumber,
              total: totalQuestions,
            })}
          </p>

          <div
            key={timeLeft}
            className="flex h-32 w-32 animate-in items-center justify-center rounded-full border-4 border-primary bg-background shadow-inner duration-300 zoom-in-50"
          >
            <span className="text-6xl font-extrabold text-primary">
              {timeLeft}
            </span>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
