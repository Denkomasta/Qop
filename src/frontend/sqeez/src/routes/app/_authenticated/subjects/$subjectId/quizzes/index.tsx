import { useState } from 'react'
import { createFileRoute, Link } from '@tanstack/react-router'
import { useTranslation } from 'react-i18next'
import { ArrowLeft, FileText } from 'lucide-react'
import {
  useGetApiSubjectsId,
  useGetApiSubjectsSubjectIdQuizzes,
} from '@/api/generated/endpoints/subjects/subjects'
import { keepPreviousData } from '@tanstack/react-query'
import { QuizListView } from '../../../quizzes/-/QuizListView'
import { Button } from '@/components/ui'
import { Badge } from '@/components/ui/Badge'
import { useAuthStore } from '@/store/useAuthStore'

export const Route = createFileRoute(
  '/app/_authenticated/subjects/$subjectId/quizzes/',
)({
  component: SubjectQuizzesPage,
})

function SubjectQuizzesPage() {
  const { t } = useTranslation()
  const { subjectId } = Route.useParams()
  const { user } = useAuthStore()

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
      StudentId: user?.id,
      IsActive: true,
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
  const titleNode = (
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
        </div>
      </div>
    </div>
  )

  return (
    <QuizListView
      titleNode={titleNode}
      quizzes={quizzes}
      totalQuizzes={totalQuizzes}
      totalPages={totalPages}
      isLoading={(isQuizzesLoading || isSubjectLoading) && !pagedResponse}
      isFetching={isFetching}
      searchQuery={searchQuery}
      setSearchQuery={setSearchQuery}
      pageNumber={pageNumber}
      setPageNumber={setPageNumber}
      subject={subjectData}
      emptyStateMessage={t('quiz.noGlobalQuizzes')}
    />
  )
}
