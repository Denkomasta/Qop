import { Link } from '@tanstack/react-router'
import { useTranslation } from 'react-i18next'
import {
  Clock,
  ChevronRight,
  PlayCircle,
  AlertCircle,
  Search,
  History,
  BookOpen,
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
import { DebouncedInput } from '@/components/ui/Input/DebouncedInput'
import { Pagination } from '@/components/ui/Pagination'
import type { QuizDto, SubjectDto } from '@/api/generated/model'

interface QuizListViewProps {
  titleNode: React.ReactNode
  backButtonNode?: React.ReactNode
  quizzes: QuizDto[]
  totalQuizzes: number
  totalPages: number
  isLoading: boolean
  isFetching: boolean
  searchQuery: string
  setSearchQuery: (query: string) => void
  pageNumber: number
  setPageNumber: (page: number) => void
  emptyStateMessage: string
  subject?: SubjectDto
}

export function QuizListView({
  titleNode,
  backButtonNode,
  quizzes,
  totalQuizzes,
  totalPages,
  isLoading,
  isFetching,
  searchQuery,
  setSearchQuery,
  pageNumber,
  setPageNumber,
  emptyStateMessage,
  subject,
}: QuizListViewProps) {
  const { t } = useTranslation()

  const formatDate = (dateString?: string | null) => {
    if (!dateString) return null
    return new Date(dateString).toLocaleDateString()
  }

  if (isLoading) {
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
          {backButtonNode}
          <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
            {titleNode}
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
          placeholder={t('quiz.search', 'Search quizzes...')}
          icon={<Search className="h-4 w-4" />}
          className="sm:max-w-xs"
        />
      </div>

      <div
        className={`transition-opacity duration-200 ${
          isFetching && !isLoading ? 'opacity-50' : 'opacity-100'
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
                      {subject && (
                        <div className="flex items-center gap-2 font-medium text-foreground">
                          <BookOpen className="h-4 w-4 text-primary" />
                          <span className="line-clamp-1">{subject.name}</span>
                        </div>
                      )}

                      {quiz.closingDate && (
                        <div className="flex items-center gap-2">
                          <Clock className="h-4 w-4" />
                          <span>
                            {t('quiz.due')}: {formatDate(quiz.closingDate)}
                          </span>
                        </div>
                      )}

                      {quiz.quizQuestions !== undefined && (
                        <div className="flex items-center gap-2">
                          <AlertCircle className="h-4 w-4" />
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
                      <PlayCircle className="h-4 w-4" />
                      {hasAttempts
                        ? quiz.maxRetries &&
                          quiz.quizAttempts >= quiz.maxRetries
                          ? t('quiz.viewResults')
                          : t('quiz.retakeQuiz')
                        : t('quiz.startQuiz')}
                      <ChevronRight className="h-4 w-4 transition-transform group-hover:translate-x-1" />
                    </Link>
                  </CardFooter>
                </Card>
              )
            })
          ) : (
            <div className="col-span-full py-12 text-center text-muted-foreground">
              {searchQuery ? t('quiz.noSearchResults') : emptyStateMessage}
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
