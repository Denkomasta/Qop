import { useTranslation } from 'react-i18next'
import { Link } from '@tanstack/react-router'
import { Trophy, Target, BarChart, ArrowRight, ListTodo } from 'lucide-react'
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardTitle,
} from '@/components/ui/Card'
import { Button } from '@/components/ui/Button'

interface QuizRecapScreenProps {
  quizId: number | string
  quizTitle: string
  totalQuestions: number
  correctCount: number
  // Optional: If you track total time spent across all questions
  // totalTimeMs?: number
}

export function QuizRecapScreen({
  quizId,
  quizTitle,
  totalQuestions,
  correctCount,
}: QuizRecapScreenProps) {
  const { t } = useTranslation()

  const scorePercentage =
    totalQuestions > 0 ? Math.round((correctCount / totalQuestions) * 100) : 0

  const isPassing = scorePercentage >= 50
  const colorClass = isPassing
    ? 'text-green-600 dark:text-green-500'
    : 'text-amber-600 dark:text-amber-500'
  const bgClass = isPassing
    ? 'bg-green-100 dark:bg-green-900/20'
    : 'bg-amber-100 dark:bg-amber-900/20'

  return (
    <div className="flex min-h-[70vh] animate-in items-center justify-center p-4 duration-500 zoom-in-95 fade-in">
      <Card className="w-full max-w-lg overflow-hidden border-primary/10 shadow-xl">
        <div
          className={`flex flex-col items-center justify-center p-8 pb-6 text-center ${bgClass}`}
        >
          <div className="mb-4 flex h-20 w-20 items-center justify-center rounded-full bg-background shadow-sm">
            <Trophy className={`h-10 w-10 ${colorClass}`} />
          </div>
          <CardTitle className="text-3xl font-extrabold tracking-tight text-foreground">
            {t('quiz.quizComplete')}
          </CardTitle>
          <CardDescription className="mt-2 text-base font-medium text-muted-foreground">
            {quizTitle}
          </CardDescription>
        </div>

        <CardContent className="space-y-8 p-8">
          <div className="flex flex-col items-center justify-center space-y-2">
            <span className="text-sm font-semibold tracking-wider text-muted-foreground uppercase">
              {t('quiz.finalScore')}
            </span>
            <div className="flex items-baseline gap-1">
              <span
                className={`text-6xl font-black tracking-tighter ${colorClass}`}
              >
                {scorePercentage}%
              </span>
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4 rounded-2xl bg-muted/30 p-4">
            <div className="flex flex-col items-center justify-center gap-1 rounded-xl bg-background p-3 shadow-sm">
              <Target className="h-5 w-5 text-primary/60" />
              <span className="text-2xl font-bold">{correctCount}</span>
              <span className="text-center text-xs font-medium text-muted-foreground">
                {t('quiz.correctAnswers')}
              </span>
            </div>

            <div className="flex flex-col items-center justify-center gap-1 rounded-xl bg-background p-3 shadow-sm">
              <ListTodo className="h-5 w-5 text-primary/60" />
              <span className="text-2xl font-bold">{totalQuestions}</span>
              <span className="text-center text-xs font-medium text-muted-foreground">
                {t('quiz.totalQuestions')}
              </span>
            </div>
          </div>
        </CardContent>

        <CardFooter className="flex flex-col gap-3 p-8 pt-0 sm:flex-row">
          <Button
            variant="outline"
            size="lg"
            className="w-full sm:w-1/2"
            asChild
          >
            <Link to="/app/quizzes">{t('quiz.backToQuizzes')}</Link>
          </Button>

          <Button size="lg" className="w-full sm:w-1/2" asChild>
            <Link
              to="/app/quizzes/$quizId/results"
              params={{ quizId: quizId.toString() }}
            >
              <BarChart className="mr-2 h-5 w-5" />
              {t('quiz.viewDetails')}
              <ArrowRight className="ml-2 h-4 w-4" />
            </Link>
          </Button>
        </CardFooter>
      </Card>
    </div>
  )
}
