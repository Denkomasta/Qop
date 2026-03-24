import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Plus } from 'lucide-react'
import { useGetApiQuizzes } from '@/api/generated/endpoints/quizzes/quizzes'
import { useGetApiSubjects } from '@/api/generated/endpoints/subjects/subjects'
import { Button } from '@/components/ui/Button'
import { ScrollableSelectList } from '@/components/ui/ScrollableSelectList/ScrollableSelectList'
import { QuizListView } from '../../../quizzes/-/QuizListView'
import { useAuthStore } from '@/store/useAuthStore'
import { CreateQuizModal } from './CreateQuizModal'

export function TeacherQuizzesPage() {
  const { t } = useTranslation()
  const { user } = useAuthStore()

  const userId = user?.id

  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false)
  const [searchQuery, setSearchQuery] = useState('')
  const [pageNumber, setPageNumber] = useState(1)
  const [selectedSubjectId, setSelectedSubjectId] = useState<string | number>(
    'all',
  )

  const { data: subjectsData, isLoading: isLoadingSubjects } =
    useGetApiSubjects({ TeacherId: userId }, { query: { enabled: !!userId } })
  const subjects = subjectsData?.data || []

  const {
    data: quizzesResponse,
    isLoading: isLoadingQuizzes,
    isFetching: isFetchingQuizzes,
  } = useGetApiQuizzes(
    {
      TeacherId: userId,
      SubjectId:
        selectedSubjectId === 'all' ? undefined : Number(selectedSubjectId),
      SearchTerm: searchQuery || undefined,
      PageNumber: pageNumber,
      PageSize: 12,
    },
    { query: { enabled: !!userId } },
  )

  const subjectOptions = [
    { id: 'all', title: t('dashboard.allSubjects') },
    ...subjects.map((subject) => ({
      id: subject.id,
      title: subject.name,
    })),
  ]

  const isAllSubjects = selectedSubjectId === 'all'

  const titleNode = (
    <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:gap-4">
      <h1 className="text-3xl font-bold tracking-tight">
        {t('dashboard.myQuizzes')}
      </h1>

      {isAllSubjects ? (
        <div
          title={t('dashboard.selectSubjectToCreate')}
          className="cursor-not-allowed"
        >
          <Button disabled size="sm" className="w-fit gap-1 shadow-md">
            <Plus className="h-4 w-4" />
            {t('dashboard.createNewQuiz')}
          </Button>
        </div>
      ) : (
        <Button
          size="sm"
          className="w-fit gap-1 shadow-md"
          onClick={() => setIsCreateModalOpen(true)}
        >
          <Plus className="h-4 w-4" />
          {t('dashboard.createNewQuiz')}
        </Button>
      )}
    </div>
  )

  return (
    <main className="flex w-full flex-1 flex-col overflow-hidden bg-background lg:flex-row">
      <aside className="w-full border-b border-border bg-muted/5 p-6 lg:w-75 lg:shrink-0 lg:border-r lg:border-b-0">
        <div className="sticky top-6">
          <h2 className="mb-4 text-sm font-black tracking-widest text-muted-foreground uppercase">
            {t('dashboard.filterBySubject')}
          </h2>
          <ScrollableSelectList
            options={subjectOptions}
            selectedId={selectedSubjectId}
            onSelect={(id) => {
              setSelectedSubjectId(id)
              setPageNumber(1)
            }}
            isLoading={isLoadingSubjects}
            loadingText={t('common.loading')}
            emptyText={t('dashboard.noSubjectsFound')}
            maxHeight="max-h-[60vh]"
          />
        </div>
      </aside>

      <section className="flex-1 overflow-y-auto">
        <QuizListView
          role="Teacher"
          titleNode={titleNode}
          quizzes={quizzesResponse?.data || []}
          totalQuizzes={Number(quizzesResponse?.totalCount || 0)}
          totalPages={Number(quizzesResponse?.totalPages || 1)}
          isLoading={isLoadingQuizzes}
          isFetching={isFetchingQuizzes}
          searchQuery={searchQuery}
          setSearchQuery={setSearchQuery}
          pageNumber={pageNumber}
          setPageNumber={setPageNumber}
          emptyStateMessage={t('dashboard.createFirstQuizPrompt')}
          subject={
            selectedSubjectId !== 'all'
              ? subjects.find((s) => s.id === selectedSubjectId)
              : undefined
          }
        />
      </section>
      <CreateQuizModal
        isOpen={isCreateModalOpen}
        onClose={() => setIsCreateModalOpen(false)}
        subjectId={selectedSubjectId}
      />
    </main>
  )
}
