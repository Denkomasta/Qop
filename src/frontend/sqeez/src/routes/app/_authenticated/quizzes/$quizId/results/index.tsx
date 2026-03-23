import { useState } from 'react'
import { createFileRoute, Link } from '@tanstack/react-router'
import { useTranslation } from 'react-i18next'
import {
  ArrowLeft,
  Clock,
  Target,
  Award,
  PlayCircle,
  History,
} from 'lucide-react'

import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/Card'
import { Button } from '@/components/ui/Button'
import { Spinner } from '@/components/ui/Spinner'
import { Badge } from '@/components/ui/Badge'
import { Pagination } from '@/components/ui/Pagination'

import { useGetApiQuizAttemptsQuizQuizId } from '@/api/generated/endpoints/quiz-attempts/quiz-attempts'
import { formatDateTime, formatDuration } from '@/lib/dateHelpers'

export const Route = createFileRoute(
  '/app/_authenticated/quizzes/$quizId/results/',
)({
  component: QuizResultsSummaryPage,
})

function QuizResultsSummaryPage() {
  const { t } = useTranslation()
  const { quizId } = Route.useParams()

  const [pageNumber, setPageNumber] = useState(1)
  const PAGE_SIZE = 10

  const { data: pagedResponse, isLoading } = useGetApiQuizAttemptsQuizQuizId(
    Number(quizId),
    { pageNumber: pageNumber, pageSize: PAGE_SIZE },
  )

  const attempts = pagedResponse?.data || []
  const totalCount = Number(pagedResponse?.totalCount || 0)
  const totalPages = Number(
    pagedResponse?.totalPages || Math.ceil(totalCount / PAGE_SIZE),
  )

  const getStatusBadge = (status: string) => {
    switch (status) {
      case 'Completed':
        return (
          <Badge variant="default" className="bg-green-500 hover:bg-green-600">
            {t('quiz.statusCompleted', 'Completed')}
          </Badge>
        )
      case 'InProgress':
        return (
          <Badge
            variant="secondary"
            className="bg-amber-500 text-white hover:bg-amber-600"
          >
            {t('quiz.statusInProgress', 'In Progress')}
          </Badge>
        )
      default:
        return <Badge variant="outline">{status}</Badge>
    }
  }

  if (isLoading) {
    return (
      <div className="flex min-h-[60vh] flex-col items-center justify-center">
        <Spinner size="lg" />
      </div>
    )
  }

  if (attempts.length === 0 && pageNumber === 1) {
    return (
      <div className="container mx-auto flex min-h-[60vh] max-w-2xl flex-col items-center justify-center space-y-4 p-6 text-center">
        <div className="flex h-20 w-20 items-center justify-center rounded-full bg-muted">
          <History className="h-10 w-10 text-muted-foreground" />
        </div>
        <h2 className="text-2xl font-bold">{t('quiz.noAttemptsTitle')}</h2>
        <p className="text-muted-foreground">{t('quiz.noAttemptsDesc')}</p>
        <Button asChild className="mt-4">
          <Link
            to={'/app/quizzes/$quizId/play'}
            params={{ quizId }}
            search={{
              attemptId: undefined,
            }}
          >
            <PlayCircle className="mr-2 h-5 w-5" />
            {t('quiz.takeQuiz')}
          </Link>
        </Button>
      </div>
    )
  }

  return (
    <div className="container mx-auto max-w-4xl animate-in space-y-8 p-4 duration-500 fade-in md:p-6 lg:p-8">
      <div>
        <Button
          variant="ghost"
          size="sm"
          asChild
          className="mb-4 -ml-3 text-muted-foreground"
        >
          <Link to="/app/quizzes">
            <ArrowLeft className="mr-2 h-4 w-4" />
            {t('common.back')}
          </Link>
        </Button>
        <h1 className="text-3xl font-bold tracking-tight text-foreground">
          {t('quiz.attemptsHistory')}
        </h1>
        <p className="text-muted-foreground">
          {t('quiz.reviewPastPerformance')}
        </p>
      </div>

      <div className="grid gap-6">
        {attempts.map((attempt, index) => {
          const isLatest = pageNumber === 1 && index === 0

          const attemptNumber =
            totalCount - (pageNumber - 1) * PAGE_SIZE - index

          return (
            <Card
              key={attempt.id}
              className={`overflow-hidden transition-all ${
                isLatest
                  ? 'border-primary shadow-md'
                  : 'border-border opacity-90 shadow-sm'
              }`}
            >
              <CardHeader
                className={`border-b pb-4 ${isLatest ? 'bg-primary/5' : 'bg-muted/20'}`}
              >
                <div className="flex items-center justify-between">
                  <div>
                    <CardTitle className="flex items-center gap-2 text-xl">
                      {isLatest
                        ? t('quiz.latestAttempt')
                        : `${t('quiz.attempt')} #${attemptNumber}`}
                      {getStatusBadge(attempt.status as string)}
                    </CardTitle>
                    <CardDescription className="mt-1.5 flex items-center gap-2">
                      <Clock className="h-4 w-4" />
                      {formatDateTime(attempt.startTime)}
                    </CardDescription>
                  </div>

                  {(attempt.status === 'Started' ||
                    attempt.status === 'Created') && (
                    <Button size="sm" asChild>
                      <Link
                        to={'/app/quizzes/$quizId/play'}
                        params={{ quizId }}
                        search={{
                          attemptId: Number(attempt.id),
                        }}
                      >
                        {t('common.resume')}
                      </Link>
                    </Button>
                  )}
                </div>
              </CardHeader>

              <CardContent className="grid gap-4 pt-6 sm:grid-cols-3">
                <div className="flex flex-col gap-1 rounded-lg border bg-card p-4 shadow-sm">
                  <div className="flex items-center gap-2 text-sm font-medium text-muted-foreground">
                    <Target className="h-4 w-4 text-primary" />
                    {t('quiz.totalScore')}
                  </div>
                  <span className="text-3xl font-bold tracking-tight text-foreground">
                    {attempt.totalScore}
                  </span>
                </div>

                <div className="flex flex-col gap-1 rounded-lg border bg-card p-4 shadow-sm">
                  <div className="flex items-center gap-2 text-sm font-medium text-muted-foreground">
                    <Award className="h-4 w-4 text-amber-500" />
                    {t('quiz.finalGrade')}
                  </div>
                  <span className="text-3xl font-bold tracking-tight text-foreground">
                    {attempt.mark !== null ? attempt.mark : '-'}
                  </span>
                </div>

                <div className="flex flex-col gap-1 rounded-lg border bg-card p-4 shadow-sm">
                  <div className="flex items-center gap-2 text-sm font-medium text-muted-foreground">
                    <Clock className="h-4 w-4 text-blue-500" />
                    {t('quiz.duration')}
                  </div>
                  <span className="text-3xl font-bold tracking-tight text-foreground">
                    {formatDuration(attempt.startTime, attempt.endTime)}
                  </span>
                </div>
              </CardContent>
            </Card>
          )
        })}
      </div>

      {totalPages > 1 && (
        <div className="mt-6 flex justify-center">
          <Pagination
            currentPage={pageNumber}
            totalPages={totalPages}
            onPageChange={setPageNumber}
          />
        </div>
      )}
    </div>
  )
}
