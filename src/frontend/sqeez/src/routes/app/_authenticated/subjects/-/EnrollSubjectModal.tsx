import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { BaseModal } from '@/components/ui/Modal'
import { AsyncButton, Button } from '@/components/ui/Button'
import { toast } from 'sonner'
import { useAuthStore } from '@/store/useAuthStore'
import {
  useGetApiSubjects,
  usePostApiSubjectsSubjectIdEnrollments as useEnrollToSubject,
} from '@/api/generated/endpoints/subjects/subjects'

interface EnrollSubjectModalProps {
  isOpen: boolean
  onClose: () => void
  onSuccess: () => void
}

export function EnrollSubjectModal({
  isOpen,
  onClose,
  onSuccess,
}: EnrollSubjectModalProps) {
  const { t } = useTranslation()
  const currentUser = useAuthStore((s) => s.user)

  const [selectedSubjectId, setSelectedSubjectId] = useState<number | ''>('')

  const { data: subjectsData, isLoading: isLoadingSubjects } =
    useGetApiSubjects(
      {},
      {
        query: { enabled: isOpen },
      },
    )

  const enrollMutation = useEnrollToSubject()

  const handleClose = () => {
    setSelectedSubjectId('')
    onClose()
  }

  const handleEnroll = async () => {
    if (!currentUser?.id || !selectedSubjectId) return

    try {
      await enrollMutation.mutateAsync({
        subjectId: selectedSubjectId,
        data: { studentIds: [currentUser.id] },
      })

      toast.success(t('enrollments.enrollSuccess'))
      onSuccess()
      handleClose()
    } catch (error) {
      console.error(error)
      toast.error(t('enrollments.enrollError'))
    }
  }

  const subjects = subjectsData?.data || []

  return (
    <BaseModal
      isOpen={isOpen}
      onClose={handleClose}
      title={t('enrollments.enrollNew')}
      description={t('enrollments.enrollDescription')}
      footer={
        <div className="flex w-full justify-center gap-4 sm:space-x-0">
          <Button
            variant="outline"
            size="lg"
            onClick={handleClose}
            className="min-w-32"
          >
            {t('common.cancel')}
          </Button>
          <AsyncButton
            size="lg"
            onClick={handleEnroll}
            disabled={!selectedSubjectId}
            loadingText={t('common.saving') + '...'}
            className="min-w-32"
          >
            {t('enrollments.confirmEnroll')}
          </AsyncButton>
        </div>
      }
    >
      <div className="py-4">
        <label
          htmlFor="subject-select"
          className="mb-2 block text-sm font-medium"
        >
          {t('enrollments.selectSubject')}
        </label>
        <select
          id="subject-select"
          value={selectedSubjectId}
          onChange={(e) => setSelectedSubjectId(Number(e.target.value))}
          disabled={isLoadingSubjects}
          className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm shadow-sm focus:border-primary focus:ring-1 focus:ring-primary focus:outline-none disabled:opacity-50"
        >
          <option value="" disabled>
            {isLoadingSubjects
              ? `${t('common.loading')}...`
              : t('enrollments.chooseOption')}
          </option>
          {subjects.map((subject) => (
            <option key={subject.id} value={subject.id}>
              {subject.name}
            </option>
          ))}
        </select>
      </div>
    </BaseModal>
  )
}
