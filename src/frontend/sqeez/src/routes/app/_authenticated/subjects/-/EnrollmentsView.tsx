import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useAuthStore } from '@/store/useAuthStore'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card'
import { Button } from '@/components/ui/Button'
import { Badge } from '@/components/ui/Badge/Badge'
import { Spinner } from '@/components/ui/Spinner'
import { BookOpen, Calendar, Plus, GraduationCap, Trash2 } from 'lucide-react'
import { toast } from 'sonner'

import { EnrollSubjectModal } from './EnrollSubjectModal'
import {
  useGetApiEnrollments,
  useDeleteApiEnrollmentsId,
} from '@/api/generated/endpoints/enrollments/enrollments'
import { Link } from '@tanstack/react-router'
import { ConfirmModal } from '@/components/ui'

export function EnrollmentsView() {
  const { t } = useTranslation()
  const currentUser = useAuthStore((s) => s.user)
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [enrollmentToDelete, setEnrollmentToDelete] = useState<number | null>(
    null,
  )

  const {
    data: enrollmentsResponse,
    isLoading,
    refetch,
  } = useGetApiEnrollments(
    {
      StudentId: currentUser?.id,
      IsActive: true,
      IsDescending: true,
    },
    {
      query: { enabled: !!currentUser?.id },
    },
  )

  const deleteMutation = useDeleteApiEnrollmentsId()

  const enrollments = enrollmentsResponse?.data || []

  const handleTrashClick = (id: number) => {
    setEnrollmentToDelete(id)
  }

  const handleConfirmDelete = async () => {
    if (!enrollmentToDelete) return

    try {
      await deleteMutation.mutateAsync({ id: enrollmentToDelete })
      toast.success(t('enrollments.unenrollSuccess'))
      refetch()
    } catch (error) {
      console.error(error)
      toast.error(t('enrollments.unenrollError'))
    } finally {
      setEnrollmentToDelete(null)
    }
  }

  if (isLoading) {
    return (
      <div className="flex min-h-[60vh] flex-col items-center justify-center gap-4">
        <Spinner size={'lg'} />
        <p className="animate-pulse font-medium text-muted-foreground">
          {t('common.loading')}...
        </p>
      </div>
    )
  }

  return (
    <div className="container mx-auto max-w-7xl p-6">
      <div className="mb-8 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="flex items-center gap-3 text-3xl font-bold tracking-tight text-foreground">
            <GraduationCap className="h-8 w-8 text-primary" />
            {t('enrollments.title')}
          </h1>
          <p className="mt-1 text-muted-foreground">
            {t('enrollments.subtitle')}
          </p>
        </div>

        <Button
          size="lg"
          onClick={() => setIsModalOpen(true)}
          className="flex items-center gap-2 shadow-sm"
        >
          <Plus className="h-5 w-5" />
          {t('enrollments.enrollButton')}
        </Button>
      </div>

      {enrollments.length > 0 ? (
        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
          {enrollments.map((enrollment) => (
            <Card
              key={enrollment.id}
              className="justify-around border-l-4 border-l-primary/60 transition-all hover:shadow-md"
            >
              <CardHeader className="pb-3">
                <CardTitle className="flex items-start justify-between gap-4 text-xl">
                  <Link
                    to="/app/subjects/$subjectId"
                    params={{
                      subjectId: (enrollment.subjectId ?? 0).toString(),
                    }}
                    className="line-clamp-2 font-bold transition-colors hover:text-primary hover:underline"
                    title={t('enrollments.goToSubject')}
                  >
                    {enrollment.subjectCode}{' '}
                    {enrollment.subjectName || 'Unknown Subject'}
                  </Link>

                  {enrollment.mark !== null && enrollment.mark !== undefined ? (
                    <Badge
                      variant="default"
                      className="shrink-0 text-sm font-bold shadow-sm"
                    >
                      {enrollment.mark}%
                    </Badge>
                  ) : (
                    <Badge
                      variant="outline"
                      className="shrink-0 text-xs font-normal text-muted-foreground"
                    >
                      {t('enrollments.noGrade')}
                    </Badge>
                  )}
                </CardTitle>
              </CardHeader>

              <CardContent>
                <div className="mt-2 flex items-center justify-between">
                  <div className="flex items-center gap-2 text-sm text-muted-foreground">
                    <Calendar className="h-4 w-4" />
                    <span>
                      {t('enrollments.enrolledOn')}{' '}
                      {new Date(enrollment.enrolledAt).toLocaleDateString()}
                    </span>
                  </div>

                  <Button
                    variant="ghost"
                    size="icon"
                    className="h-8 w-8 text-muted-foreground transition-colors hover:bg-destructive/10 hover:text-destructive"
                    onClick={() => handleTrashClick(Number(enrollment.id))}
                    title={t('common.delete')}
                  >
                    <Trash2 className="h-4 w-4" />
                  </Button>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      ) : (
        <div className="flex min-h-[40vh] flex-col items-center justify-center rounded-xl border border-dashed border-muted-foreground/30 bg-secondary/10 p-8 text-center">
          <BookOpen className="mb-4 h-16 w-16 text-muted-foreground/40" />
          <h2 className="mb-2 text-xl font-semibold text-foreground">
            {t('enrollments.emptyTitle')}
          </h2>
          <p className="mb-6 max-w-md text-muted-foreground">
            {t('enrollments.emptyDescription')}
          </p>
          <Button variant="outline" onClick={() => setIsModalOpen(true)}>
            <Plus className="mr-2 h-4 w-4" />
            {t('enrollments.enrollButton')}
          </Button>
        </div>
      )}

      <EnrollSubjectModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        onSuccess={() => refetch()}
      />

      <ConfirmModal
        isOpen={enrollmentToDelete !== null}
        onClose={() => setEnrollmentToDelete(null)}
        onConfirm={handleConfirmDelete}
        title={t('common.confirmAction')}
        description={t('enrollments.confirmDelete')}
        confirmText={t('common.delete')}
        isDestructive={true}
        isLoading={deleteMutation.isPending}
      />
    </div>
  )
}
