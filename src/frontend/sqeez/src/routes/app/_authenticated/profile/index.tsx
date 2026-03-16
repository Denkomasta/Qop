import { useState } from 'react'
import { createFileRoute } from '@tanstack/react-router'
import { useAuthStore } from '@/store/useAuthStore'
import { useExtendedUserProfile } from '@/hooks/useExtendedUserProfile'
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/Avatar'
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/Card'
import { Input } from '@/components/ui/Input'
import {
  Mail,
  Phone,
  Briefcase,
  Shield,
  Star,
  User as UserIcon,
} from 'lucide-react'
import { useTranslation } from 'react-i18next'
// import { toast } from 'sonner'

import { EditableInfoItem } from '@/components/ui/InfoItem'
import { AsyncButton, Button } from '@/components/ui/Button'
import { BaseModal } from '@/components/ui/Modal'
import {
  useUpdateProfile,
  type ProfilePatchPayload,
} from '@/hooks/useUpdateProfile'
import { Spinner } from '@/components/ui/Spinner'

export const Route = createFileRoute('/app/_authenticated/profile/')({
  component: ProfilePage,
})

type EditFieldState = {
  key: string
  label: string
  value: string
} | null

function ProfilePage() {
  const { t } = useTranslation()
  const user = useAuthStore((s) => s.user)

  const { data: extendedData, isLoading } = useExtendedUserProfile(
    user?.id,
    user?.role,
  )

  const updateProfile = useUpdateProfile(user?.id, user?.role)

  const [isModalOpen, setIsModalOpen] = useState(false)
  const [editingField, setEditingField] = useState<EditFieldState>(null)
  const [editValue, setEditValue] = useState('')

  if (!user) return null

  const initials = user.username.substring(0, 2).toUpperCase()

  const handleEditClick = (
    key: string,
    label: string,
    currentValue: string,
  ) => {
    setEditingField({ key, label, value: currentValue })
    setEditValue(currentValue)
    setIsModalOpen(true)
  }

  const handleCloseModal = () => {
    setIsModalOpen(false)
    setEditingField(null)
  }

  const handleSave = async () => {
    if (!editingField) return

    try {
      const payload = { [editingField.key]: editValue } as ProfilePatchPayload

      await updateProfile.mutateAsync(payload)

      // toast.success(t('common.success'), {
      //   description: `${editingField.label} has been successfully updated.`,
      // })

      handleCloseModal()
    } catch (error) {
      console.error(error)
      // toast.error(t('common.error'), {
      //   description: 'Failed to update your profile. Please try again.',
      // })
    }
  }

  return (
    <div className="container mx-auto max-w-7xl p-6">
      <h1 className="mb-8 text-3xl font-bold tracking-tight text-foreground">
        {t('profile.title')}
      </h1>

      <div className="grid gap-6 md:grid-cols-3">
        <Card className="shadow-sm md:col-span-1">
          <CardContent className="flex flex-col items-center pt-8">
            <Avatar className="size-32 border-4 border-primary/10">
              {user.avatarUrl !== null ? (
                <AvatarImage
                  src={user.avatarUrl}
                  alt={`${user.username}'s avatar`}
                  className="object-cover"
                />
              ) : (
                <AvatarFallback className="bg-primary text-4xl font-bold text-primary-foreground">
                  {initials}
                </AvatarFallback>
              )}
            </Avatar>
            <h2 className="mt-5 text-2xl font-bold">{user.username}</h2>
            <p className="text-sm text-muted-foreground">{user.email}</p>
            <div className="mt-6 flex items-center justify-center gap-2 rounded-full bg-secondary/80 px-4 py-1.5 text-sm font-semibold text-secondary-foreground shadow-sm">
              <Star className="h-4 w-4 fill-yellow-500 text-yellow-500" />
              {user.currentXP} {t('common.xp')}
            </div>
          </CardContent>
        </Card>

        {/* Right Column: Detailed Account Info */}
        <Card className="shadow-sm md:col-span-2">
          <CardHeader>
            <CardTitle className="text-xl">
              {t('profile.accountDetails')}
            </CardTitle>
            <CardDescription className="text-lg">
              {t('profile.description')}
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-6">
            {isLoading ? (
              <p className="text-sm text-muted-foreground">
                <Spinner size="lg" className="text-primary" />
              </p>
            ) : (
              <div className="grid gap-4 sm:grid-cols-2">
                <EditableInfoItem
                  icon={<UserIcon className="h-4 w-4" />}
                  label={t('common.username')}
                  value={user.username}
                  fieldKey="username"
                  onEdit={handleEditClick}
                />
                <EditableInfoItem
                  icon={<Mail className="h-4 w-4" />}
                  label={t('common.email')}
                  value={user.email}
                  fieldKey="email"
                  onEdit={handleEditClick}
                />
                <EditableInfoItem
                  icon={<Shield className="h-4 w-4" />}
                  label={t('common.role')}
                  value={user.role}
                  fieldKey="role"
                  canEdit={false}
                  onEdit={handleEditClick}
                />
                <>
                  {extendedData &&
                    'department' in extendedData &&
                    extendedData.department && (
                      <EditableInfoItem
                        icon={<Briefcase className="h-4 w-4" />}
                        label={t('common.department')}
                        value={extendedData.department}
                        fieldKey="department"
                        onEdit={handleEditClick}
                      />
                    )}
                  {extendedData &&
                    'phoneNumber' in extendedData &&
                    extendedData.phoneNumber && (
                      <EditableInfoItem
                        icon={<Phone className="h-4 w-4" />}
                        label={t('common.phoneNumber')}
                        value={extendedData.phoneNumber}
                        fieldKey="phoneNumber"
                        onEdit={handleEditClick}
                      />
                    )}
                </>
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* --- Generic Edit Modal --- */}
      <BaseModal
        isOpen={isModalOpen}
        onClose={handleCloseModal}
        title={`${t('common.edit')} ${editingField?.label.toLowerCase()}`}
        description={t('profile.editModalDescription', {
          field: editingField?.label?.toLowerCase(),
        })}
        footer={
          <div className="flex w-full justify-center gap-4 sm:space-x-0">
            <Button
              variant="outline"
              size="lg"
              onClick={handleCloseModal}
              className="min-w-32"
            >
              {t('common.cancel')}
            </Button>

            <AsyncButton
              size="lg"
              onClick={handleSave}
              loadingText={t('common.saving') + '...'}
              className="min-w-32"
            >
              {t('common.save')}
            </AsyncButton>
          </div>
        }
      >
        <div className="grid gap-4">
          <div className="flex flex-col gap-2">
            <label htmlFor="edit-input" className="text-sm font-medium">
              {editingField?.label}
            </label>
            <Input
              id="edit-input"
              value={editValue}
              onChange={(e) => setEditValue(e.target.value)}
              autoFocus
            />
          </div>
        </div>
      </BaseModal>
    </div>
  )
}
