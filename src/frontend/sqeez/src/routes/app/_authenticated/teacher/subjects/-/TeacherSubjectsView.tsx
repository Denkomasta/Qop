import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useAuthStore } from '@/store/useAuthStore'
import { Button } from '@/components/ui/Button'
import {
  BookCopy,
  Users,
  Settings,
  Library,
  FileSignature,
  School,
  Plus,
} from 'lucide-react'
import { Link } from '@tanstack/react-router'

import { useGetApiSubjects } from '@/api/generated/endpoints/subjects/subjects'
import type { SubjectDto } from '@/api/generated/model'
import { SubjectCard } from '@/components/ui/Card'
import { PaginatedListView } from '@/components/layouting/PaginatedListView/PaginatedListView'

export function TeacherSubjectsView() {
  const { t } = useTranslation()
  const currentUser = useAuthStore((s) => s.user)

  const [page, setPage] = useState(1)
  const pageSize = 12

  const {
    data: subjectsResponse,
    isLoading,
    isFetching,
  } = useGetApiSubjects(
    {
      TeacherId: currentUser?.id,
      PageNumber: page,
      PageSize: pageSize,
    },
    {
      query: {
        enabled: !!currentUser?.id,
        placeholderData: (previousData) => previousData,
      },
    },
  )

  const subjects: SubjectDto[] = subjectsResponse?.data || []
  const totalCount = Number(subjectsResponse?.totalCount || 0)
  const totalPages = Number(subjectsResponse?.totalPages || 1)

  return (
    <PaginatedListView<SubjectDto>
      titleNode={
        <h1 className="flex items-center gap-3 text-3xl font-bold tracking-tight text-foreground">
          <Library className="h-8 w-8 text-primary" />
          {t('teacherSubjects.title')}
        </h1>
      }
      isLoading={isLoading && !subjectsResponse}
      isFetching={isFetching}
      items={subjects}
      totalCount={totalCount}
      pageNumber={page}
      totalPages={totalPages}
      setPageNumber={setPage}
      emptyStateMessage={
        <div className="flex flex-col items-center gap-2">
          <BookCopy className="mb-4 h-16 w-16 text-muted-foreground/40" />
          <h2 className="text-xl font-semibold text-foreground">
            {t('teacherSubjects.emptyTitle')}
          </h2>
          <p className="max-w-md text-muted-foreground">
            {t('teacherSubjects.emptyDescription')}
          </p>
        </div>
      }
      renderItem={(subject) => (
        <SubjectCard
          key={subject.id}
          title={subject.name}
          code={subject.code}
          url="/app/teacher/subjects/$subjectId"
          params={{ subjectId: subject.id.toString() }}
          borderColorClass="border-l-blue-500/60"
          badgeColorClass="bg-blue-500/10 text-blue-600"
          description={subject.description || t('common.noDescription')}
          topRightSlot={
            subject.schoolClassName ? (
              <div className="flex items-center gap-1 text-xs text-muted-foreground">
                <School className="h-3 w-3" />
                {subject.schoolClassName}
              </div>
            ) : null
          }
          metricsSlot={
            <>
              <div
                className="flex items-center gap-1.5"
                title={t('teacherSubjects.enrolledCount')}
              >
                <Users className="h-4 w-4" />
                <span className="font-medium text-foreground">
                  {subject.enrollmentCount}
                </span>
              </div>
              <div
                className="flex items-center gap-1.5"
                title={t('teacherSubjects.quizCount')}
              >
                <FileSignature className="h-4 w-4" />
                <span className="font-medium text-foreground">
                  {subject.quizCount}
                </span>
              </div>
            </>
          }
          actionsSlot={
            <Link
              to="/app/teacher/subjects/$subjectId/settings"
              params={{ subjectId: subject.id.toString() }}
            >
              <Button
                variant="ghost"
                size="sm"
                className="gap-2 text-muted-foreground transition-colors hover:bg-secondary/80 hover:text-foreground"
              >
                <Settings className="h-4 w-4" />
                {t('common.settings')}
              </Button>
            </Link>
          }
        />
      )}
    />
  )
}
