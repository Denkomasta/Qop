import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from '@tanstack/react-router'
import { BookCopy, ArrowLeft, Search, UserPlus } from 'lucide-react'

import {
  getGetApiSubjectsIdQueryKey,
  useDeleteApiSubjectsSubjectIdEnrollments,
  useGetApiSubjectsId,
} from '@/api/generated/endpoints/subjects/subjects'
import {
  getGetApiUsersQueryKey,
  useGetApiUsers,
} from '@/api/generated/endpoints/user/user'

import { Button } from '@/components/ui/Button'
import { DebouncedInput } from '@/components/ui/Input/DebouncedInput'
import { Pagination } from '@/components/ui/Pagination'
import { Badge } from '@/components/ui/Badge/Badge'
import { SubjectStudentsTable } from './SubjectStudentsTable'
import { useAuthStore } from '@/store/useAuthStore'
import { useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import type { StudentDto } from '@/api/generated/model'
import { ConfirmModal } from '@/components/ui'
import { formatName } from '@/lib/userHelpers'
import { StudentGradingModal } from './StudentGradingModal'

export function SubjectDetailsPage({
  subjectId,
}: {
  subjectId: string | number
}) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()

  const { isAdmin } = useAuthStore()

  const [searchQuery, setSearchQuery] = useState('')
  const [pageNumber, setPageNumber] = useState(1)
  const [studentToRemove, setStudentToRemove] = useState<StudentDto | null>(
    null,
  )
  const [studentToGrade, setStudentToGrade] = useState<StudentDto | null>(null)

  const pageSize = 15

  const { data: subjectData, isLoading: isLoadingSubject } =
    useGetApiSubjectsId(Number(subjectId))

  const { data: studentsResponse, isLoading: isLoadingStudents } =
    useGetApiUsers({
      Role: 'Student',
      SubjectId: Number(subjectId),
      SearchTerm: searchQuery || undefined,
      PageNumber: pageNumber,
      PageSize: pageSize,
    })

  const removeStudentMutation = useDeleteApiSubjectsSubjectIdEnrollments({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiUsersQueryKey() })
        queryClient.invalidateQueries({
          queryKey: getGetApiSubjectsIdQueryKey(Number(subjectId)),
        })

        toast.success(t('subject.studentRemoved'))
        setStudentToRemove(null)
      },
      onError: () => toast.error(t('common.error')),
    },
  })

  const handleRemoveConfirm = async () => {
    if (!studentToRemove?.id) return

    try {
      await removeStudentMutation.mutateAsync({
        subjectId: Number(subjectId),
        data: {
          studentIds: [studentToRemove.id],
        },
      })
    } catch (error) {
      console.error('Failed to remove student:', error)
    }
  }

  const canEditEnrollments =
    isAdmin || subjectData?.teacherId === useAuthStore.getState().user?.id

  const isLoading = isLoadingSubject || isLoadingStudents
  const students = studentsResponse?.data || []
  const totalPages = Number(studentsResponse?.totalPages || 1)
  const totalCount = studentsResponse?.totalCount || 0

  return (
    <div className="flex h-full flex-col bg-background">
      <div className="border-b border-border bg-card p-6">
        <div className="mx-auto flex max-w-7xl flex-col gap-6">
          <Link
            to="/app/subjects"
            className="flex w-fit items-center gap-2 text-sm text-muted-foreground hover:text-foreground"
          >
            <ArrowLeft className="h-4 w-4" />
            {t('subject.backToSubjects')}
          </Link>

          <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
            <div className="flex items-center gap-4">
              <div className="flex h-16 w-16 items-center justify-center rounded-xl bg-cyan-500/10 text-cyan-500">
                <BookCopy className="h-8 w-8" />
              </div>
              <div>
                <div className="flex items-center gap-2">
                  <h1 className="text-3xl font-bold tracking-tight">
                    {subjectData?.name}
                  </h1>
                  <Badge variant="secondary" className="uppercase">
                    {subjectData?.code}
                  </Badge>
                </div>
                <div className="mt-2 flex flex-wrap items-center gap-x-4 gap-y-2 text-sm text-muted-foreground">
                  <span>
                    {t('common.teacher')}:{' '}
                    <strong className="text-foreground">
                      {subjectData?.teacherName || t('admin.unassigned')}
                    </strong>
                  </span>
                  <span>
                    {t('admin.schoolClass')}:{' '}
                    <strong className="text-foreground">
                      {subjectData?.schoolClassName || t('admin.unassigned')}
                    </strong>
                  </span>
                </div>
              </div>
            </div>

            {isAdmin && (
              <Button variant="outline" className="gap-2">
                <UserPlus className="h-4 w-4" />
                {t('subject.enrollStudent')}
              </Button>
            )}
          </div>

          <div className="flex items-center">
            <DebouncedInput
              id="subject-student-search"
              value={searchQuery}
              onChange={(val) => {
                setSearchQuery(val)
                setPageNumber(1)
              }}
              placeholder={t('admin.searchStudents')}
              icon={<Search className="h-4 w-4" />}
              className="max-w-md bg-background"
              hideErrors
            />
          </div>
        </div>
      </div>

      <div className="flex-1 overflow-y-auto p-6">
        <div className="mx-auto max-w-7xl">
          <div className="mb-4 flex items-center justify-between">
            <h2 className="text-lg font-semibold">
              {t('subject.enrolledStudents')}
            </h2>
            <span className="text-sm text-muted-foreground">
              {totalCount} {t('common.students')}
            </span>
          </div>

          <SubjectStudentsTable
            students={students}
            isLoading={isLoading}
            onRemoveStudent={
              canEditEnrollments ? setStudentToRemove : undefined
            }
            onEditMark={canEditEnrollments ? setStudentToGrade : undefined}
          />

          {!isLoading && totalPages > 1 && (
            <div className="mt-6 flex justify-center">
              <Pagination
                currentPage={pageNumber}
                totalPages={totalPages}
                onPageChange={setPageNumber}
              />
            </div>
          )}
        </div>
      </div>
      <ConfirmModal
        isOpen={!!studentToRemove}
        onClose={() => setStudentToRemove(null)}
        onConfirm={handleRemoveConfirm}
        title={t('subject.removeFromSubject')}
        description={t('subject.removeConfirmDesc', {
          studentName: studentToRemove
            ? formatName(studentToRemove.firstName, studentToRemove.lastName)
            : '',
        })}
        confirmText={t('common.remove')}
        isDestructive={true}
        isLoading={removeStudentMutation.isPending}
      />

      <StudentGradingModal
        isOpen={!!studentToGrade}
        onClose={() => setStudentToGrade(null)}
        student={studentToGrade}
        subjectId={Number(subjectId)}
      />
    </div>
  )
}
