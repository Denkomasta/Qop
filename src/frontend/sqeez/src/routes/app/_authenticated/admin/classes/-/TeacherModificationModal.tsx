import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { useQueryClient } from '@tanstack/react-query'

import { usePatchApiClassesId } from '@/api/generated/endpoints/school-classes/school-classes'
import { getGetApiClassesQueryKey } from '@/api/generated/endpoints/school-classes/school-classes'

import { BaseModal } from '@/components/ui/Modal'
import { Button, AsyncButton } from '@/components/ui/Button'
import { formatName } from '@/lib/userHelpers'
import type { SchoolClassDto } from '@/api/generated/model'
import { useGetApiUsers } from '@/api/generated/endpoints/user/user'

interface TeacherModificationModalProps {
  isOpen: boolean
  onClose: () => void
  schoolClass: SchoolClassDto | null
}

export function TeacherModificationModal({
  isOpen,
  onClose,
  schoolClass,
}: TeacherModificationModalProps) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()

  const [selectedTeacherId, setSelectedTeacherId] = useState<
    string | number | ''
  >(schoolClass?.teacherId ?? '')

  const { data: teachersResponse, isLoading: isLoadingTeachers } =
    useGetApiUsers(
      { Role: 'Teacher', PageSize: 100 },
      { query: { enabled: isOpen } },
    )
  const teachers = teachersResponse?.data || []

  const updateClassMutation = usePatchApiClassesId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiClassesQueryKey() })
        toast.success(t('admin.classes.teacherUpdated'))
        onClose()
      },
      onError: () => toast.error(t('common.error')),
    },
  })

  const handleSave = async () => {
    if (!schoolClass) return

    try {
      await updateClassMutation.mutateAsync({
        id: schoolClass.id.toString(),
        data: {
          teacherId:
            selectedTeacherId === '' ? null : Number(selectedTeacherId),
        },
      })
      console.log(
        `Assigned teacher ID ${selectedTeacherId} to class ${schoolClass.name}`,
      )
      onClose()
    } catch (error) {
      console.error('Failed to assign teacher:', error)
    }
  }

  return (
    <BaseModal
      isOpen={isOpen}
      onClose={onClose}
      title={t('admin.classes.assignTeacherTitle')}
      description={t('admin.classes.assignTeacherDesc', {
        className: schoolClass?.name,
      })}
      footer={
        <div className="flex w-full justify-between gap-4">
          <Button
            variant="outline"
            size="lg"
            onClick={onClose}
            className="min-w-32"
          >
            {t('common.cancel')}
          </Button>
          <AsyncButton
            size="lg"
            onClick={handleSave}
            disabled={selectedTeacherId === (schoolClass?.teacherId ?? '')}
            isLoading={updateClassMutation.isPending}
            loadingText={t('common.saving') + '...'}
            className="min-w-32"
          >
            {t('common.save')}
          </AsyncButton>
        </div>
      }
    >
      <div className="flex flex-col gap-2 py-4">
        <label className="text-sm font-medium text-foreground">
          {t('admin.classes.selectTeacher')}
        </label>
        <select
          className="h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background focus-visible:ring-2 focus-visible:ring-ring focus-visible:outline-none"
          value={selectedTeacherId}
          onChange={(e) => setSelectedTeacherId(e.target.value)}
          disabled={isLoadingTeachers}
        >
          <option value="">-- {t('admin.classes.unassigned')} --</option>
          {teachers.map((teacher) => (
            <option key={teacher.id} value={teacher.id}>
              {formatName(teacher.firstName, teacher.lastName)} (@
              {teacher.username})
            </option>
          ))}
        </select>
      </div>
    </BaseModal>
  )
}
