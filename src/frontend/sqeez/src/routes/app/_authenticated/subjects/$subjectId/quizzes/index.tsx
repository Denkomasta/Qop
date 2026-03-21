import { useState } from 'react'
import { createFileRoute, Link } from '@tanstack/react-router'
import { useTranslation } from 'react-i18next'
import {
  FileText,
  Clock,
  ChevronRight,
  PlayCircle,
  AlertCircle,
  Search,
  ArrowLeft,
  History,
} from 'lucide-react'
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
  CardFooter,
} from '@/components/ui/Card'
import { Badge } from '@/components/ui/Badge/Badge'
import { Spinner } from '@/components/ui/Spinner'
import { Button } from '@/components/ui/Button'
import { DebouncedInput } from '@/components/ui/Input/DebouncedInput'
import { Pagination } from '@/components/ui/Pagination'
import {
  useGetApiSubjectsId,
  useGetApiSubjectsSubjectIdQuizzes,
} from '@/api/generated/endpoints/subjects/subjects'
import { keepPreviousData } from '@tanstack/react-query'
import { formatDate } from '@/lib/dateHelpers'

export const Route = createFileRoute(
  '/app/_authenticated/subjects/$subjectId/quizzes/',
)({
  component: SubjectQuizzesPage,
})

function SubjectQuizzesPage() {
  const { t } = useTranslation()
  const { subjectId } = Route.useParams()

  const [searchQuery, setSearchQuery] = useState('')
  const [pageNumber, setPageNumber] = useState(1)
  const PAGE_SIZE = 12

  const { data: subjectData, isLoading: isSubjectLoading } =
    useGetApiSubjectsId(Number(subjectId), {
      query: { enabled: !!subjectId },
    })

  const {
    data: pagedResponse,
    isLoading: isQuizzesLoading,
    isFetching,
  } = useGetApiSubjectsSubjectIdQuizzes(
    Number(subjectId),
    {
      SearchTerm: searchQuery,
      PageNumber: pageNumber,
      PageSize: PAGE_SIZE,
    },
    {
      query: {
        enabled: !!subjectId,
        placeholderData: keepPreviousData,
      },
    },
  )

  const quizzes = pagedResponse?.data || []
  const totalQuizzes = Number(pagedResponse?.totalCount || 0)
  const totalPages = Number(
    pagedResponse?.totalPages || Math.ceil(totalQuizzes / PAGE_SIZE),
  )

  if ((isQuizzesLoading && !pagedResponse) || isSubjectLoading) {
    return (
      <div className="flex min-h-[60vh] flex-col items-center justify-center gap-4">
        <Spinner size="lg" />
        <p className="animate-pulse font-medium text-muted-foreground">
          {t('common.loading', 'Loading')}...
        </p>
      </div>
    )
  }

  return (
    <div className="container mx-auto max-w-7xl p-6">
      <div className="mb-8 flex flex-col gap-4">
        <div>
          <Button variant="ghost" size="sm" asChild className="mb-4 -ml-3">
            <Link to="/app/subjects/$subjectId" params={{ subjectId }}>
              <ArrowLeft className="mr-2 size-4" />
              {t('subject.backToSubject')}
            </Link>
          </Button>
          <div className="flex flex-col gap-2 sm:flex-row sm:items-end sm:justify-between">
            <h1 className="flex items-center gap-3 text-3xl font-bold tracking-tight text-foreground">
              <FileText className="size-8 text-primary" />
              <span>
                {subjectData?.name}
                <span className="ml-2 font-normal text-muted-foreground">
                  | {t('quiz.subjectQuizzes')}
                </span>
              </span>
              {subjectData?.code && (
                <Badge variant="secondary" className="mb-1 ml-2 text-sm">
                  {subjectData.code}
                </Badge>
              )}
            </h1>
            <p className="font-medium text-muted-foreground">
              {t('quiz.totalCount', { count: totalQuizzes })}
            </p>
          </div>
        </div>
      </div>

      <div className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <DebouncedInput
          id="quiz-search"
          value={searchQuery}
          onChange={(newQuery) => {
            setSearchQuery(newQuery)
            setPageNumber(1)
          }}
          placeholder={t('quiz.search')}
          icon={<Search className="size-4" />}
          className="sm:max-w-xs"
        />
      </div>

      <div
        className={`transition-opacity duration-200 ${
          isFetching && !isQuizzesLoading ? 'opacity-50' : 'opacity-100'
        }`}
      >
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {quizzes.length > 0 ? (
            quizzes.map((quiz) => {
              const hasAttempts = Number(quiz.quizAttempts) > 0

              return (
                <Card
                  key={quiz.id}
                  className="flex flex-col transition-shadow hover:shadow-md"
                >
                  <CardHeader className="pb-3">
                    <div className="flex items-start justify-between gap-4">
                      <CardTitle className="line-clamp-2 text-lg">
                        {quiz.title}
                      </CardTitle>

                      <Badge
                        variant={hasAttempts ? 'default' : 'secondary'}
                        className="shrink-0"
                      >
                        {hasAttempts ? t('quiz.completed') : t('quiz.pending')}
                      </Badge>
                    </div>
                    <CardDescription className="mt-1 line-clamp-2">
                      {quiz.description || t('quiz.noDescription')}
                    </CardDescription>
                  </CardHeader>

                  <CardContent className="flex-1 pb-3">
                    <div className="flex flex-col gap-2 text-sm text-muted-foreground">
                      {quiz.closingDate && (
                        <div className="flex items-center gap-2">
                          <Clock className="size-4" />
                          <span>
                            {t('quiz.due')}: {formatDate(quiz.closingDate)}
                          </span>
                        </div>
                      )}
                      {quiz.quizQuestions !== undefined && (
                        <div className="flex items-center gap-2">
                          <AlertCircle className="size-4" />
                          <span>
                            {quiz.quizQuestions} {t('quiz.questions')}
                          </span>
                        </div>
                      )}

                      {quiz.quizAttempts !== undefined && (
                        <div className="flex items-center gap-2">
                          <History className="h-4 w-4" />
                          <span>
                            {quiz.quizAttempts}
                            {quiz.maxRetries
                              ? ` / ${quiz.maxRetries}`
                              : ''}{' '}
                            {t('quiz.attempts')}
                          </span>
                        </div>
                      )}
                    </div>
                  </CardContent>

                  <CardFooter className="pt-3">
                    <Link
                      to="/app/quizzes/$quizId"
                      params={{ quizId: quiz.id.toString() }}
                      className="group flex w-full items-center justify-center gap-2 rounded-md bg-secondary px-4 py-2 text-sm font-medium text-secondary-foreground transition-colors hover:bg-secondary/80"
                    >
                      <PlayCircle className="size-4" />

                      {hasAttempts
                        ? t('quiz.viewResults')
                        : t('quiz.startQuiz')}

                      <ChevronRight className="size-4 transition-transform group-hover:translate-x-1" />
                    </Link>
                  </CardFooter>
                </Card>
              )
            })
          ) : (
            <div className="col-span-full py-12 text-center text-muted-foreground">
              {searchQuery ? t('quiz.noSearchResults') : t('quiz.noQuizzes')}
            </div>
          )}
        </div>

        {totalPages > 1 && (
          <div className="mt-8 flex justify-center">
            <Pagination
              currentPage={pageNumber}
              totalPages={totalPages}
              onPageChange={setPageNumber}
            />
          </div>
        )}
      </div>
    </div>
  )
}
