import { useState } from 'react'
import { useAuthStore } from '@/store/useAuthStore'
import { SimpleAvatar } from '@/components/ui/Avatar'
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

import { EditableInfoItem } from '@/components/ui/InfoItem'
import { AsyncButton, Button } from '@/components/ui/Button'
import { BaseModal } from '@/components/ui/Modal'
import { Spinner } from '@/components/ui/Spinner'
import { toast } from 'sonner'
import { Badge } from '@/components/ui/Badge/Badge'
import { calculateLevel, formatName } from '@/lib/userHelpers'
import {
  useGetApiUsersIdDetails,
  usePatchApiUsersId,
} from '@/api/generated/endpoints/user/user'
import type { PatchStudentDto, UserRole } from '@/api/generated/model'

type EditFieldState = {
  key: string
  label: string
  value: string
} | null

export function ProfileView({ targetUserId }: { targetUserId?: number }) {
  const { t } = useTranslation()
  const currentUser = useAuthStore((s) => s.user)

  const idToFetch = targetUserId || currentUser?.id

  const isOwnProfile = currentUser?.id === idToFetch

  const { data: profileData, isLoading: isLoading } = useGetApiUsersIdDetails(
    currentUser?.id ?? 0,
    { query: { enabled: !!currentUser?.id } },
  )

  const updateProfile = usePatchApiUsersId()

  const [isModalOpen, setIsModalOpen] = useState(false)
  const [editingField, setEditingField] = useState<EditFieldState>(null)
  const [editValue, setEditValue] = useState('')
  const [editError, setEditError] = useState<string | undefined>(undefined)

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

  if (!profileData) {
    return (
      <div className="container mx-auto p-6 text-center text-muted-foreground">
        {t('errors.userNotFound')}
      </div>
    )
  }

  const validateField = (key: string, value: string): string | null => {
    const trimmedValue = value.trim()

    if (!trimmedValue) return t('errors.required')

    switch (key) {
      case 'email': {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
        if (!emailRegex.test(trimmedValue)) return t('errors.invalidEmail')
        break
      }

      case 'username': {
        const safeUsernameRegex =
          /^[a-zA-Z0-9_\-áéíóúýčďěňřšťžÁÉÍÓÚÝČĎĚŇŘŠŤŽ]+$/

        if (trimmedValue.length < 3) return t('errors.usernameShort')
        if (trimmedValue.length > 20) return t('errors.usernameLong')
        if (!safeUsernameRegex.test(trimmedValue)) {
          return t('errors.invalidCharacters')
        }
        break
      }

      case 'phoneNumber': {
        const phoneRegex = /^\+?[0-9\s\-()]{7,15}$/
        if (trimmedValue && !phoneRegex.test(trimmedValue))
          return t('errors.invalidPhone')
        break
      }
    }

    return null
  }

  const handleEditClick = (
    key: string,
    label: string,
    currentValue: string,
  ) => {
    if (!isOwnProfile) return

    setEditingField({ key, label, value: currentValue })
    setEditValue(currentValue)
    setIsModalOpen(true)
  }

  const handleCloseModal = () => {
    setIsModalOpen(false)
    setEditingField(null)
    setEditError(undefined)
  }

  const handleSave = async () => {
    if (!editingField || !isOwnProfile || !currentUser || !profileData.role)
      return

    const validationError = validateField(editingField.key, editValue)
    if (validationError) {
      setEditError(validationError)
      return
    }

    try {
      const discriminatorMap: Record<
        UserRole,
        'student' | 'teacher' | 'admin'
      > = {
        Student: 'student',
        Teacher: 'teacher',
        Admin: 'admin',
      }

      const discriminator =
        discriminatorMap[profileData.role as UserRole] ?? 'student'

      const payload: PatchStudentDto = {
        role: discriminator,
        [editingField.key]: editValue.trim(),
      }

      await updateProfile.mutateAsync({
        id: currentUser.id,
        data: payload,
      })

      toast.success(t('common.successfulChange'))
      handleCloseModal()
    } catch (error) {
      console.error(error)
      toast.error(t('common.unsuccessfulChange'))
    }
  }

  return (
    <div className="container mx-auto max-w-7xl p-6">
      <h1 className="mb-8 flex items-center gap-3 text-3xl font-bold tracking-tight text-foreground">
        {isOwnProfile
          ? t('profile.title')
          : t('profile.userTitle', {
              name: formatName(profileData.firstName, profileData.lastName),
            })}
        {isOwnProfile && (
          <Badge
            variant="outline"
            className="ml-2 border-primary/20 bg-primary/10 text-primary"
          >
            {t('class.me')}
          </Badge>
        )}
      </h1>

      <div className="grid gap-6 md:grid-cols-3">
        <Card className="shadow-sm md:col-span-1">
          <CardContent className="flex flex-col items-center pt-8">
            <SimpleAvatar
              url={profileData.avatarUrl}
              firstName={profileData.firstName}
              lastName={profileData.lastName}
              wrapperClassName="size-32 border-4 border-primary/10"
              imageClassName="object-cover"
              fallbackClassName="bg-primary text-4xl font-bold text-primary-foreground"
            />
            <h2 className="mt-5 text-center text-2xl font-bold">
              {formatName(profileData.firstName, profileData.lastName)}
            </h2>
            <p className="mt-1 text-sm text-muted-foreground">
              @{profileData.username}
            </p>

            <div className="mt-6 flex w-full flex-col gap-3">
              <div className="flex items-center justify-center gap-2 rounded-full bg-secondary/80 px-4 py-2 text-sm font-semibold text-secondary-foreground shadow-sm">
                <Star className="h-4 w-4 fill-yellow-500 text-yellow-500" />
                {t('class.level', {
                  level: calculateLevel(profileData.currentXP),
                })}
                <span className="ml-1 font-normal text-muted-foreground">
                  ({profileData.currentXP ?? 0} {t('common.xp')})
                </span>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Right Column: Detailed Account Info */}
        <Card className="h-fit shadow-sm md:col-span-2">
          <CardHeader>
            <CardTitle className="text-xl">
              {t('profile.accountDetails')}
            </CardTitle>
            <CardDescription className="text-base">
              {isOwnProfile
                ? t('profile.description')
                : t('profile.viewOnlyDescription')}
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-6">
            <div className="grid gap-4 sm:grid-cols-2">
              <EditableInfoItem
                icon={<UserIcon className="h-4 w-4" />}
                label={t('common.username')}
                value={profileData.username ?? 'john_doe'}
                fieldKey="username"
                canEdit={isOwnProfile}
                onEdit={handleEditClick}
              />
              <EditableInfoItem
                icon={<Mail className="h-4 w-4" />}
                label={t('common.email')}
                value={profileData.email ?? 'john_doe@sqeez.com'}
                fieldKey="email"
                canEdit={isOwnProfile}
                onEdit={handleEditClick}
              />
              <EditableInfoItem
                icon={<Shield className="h-4 w-4" />}
                label={t('common.role')}
                value={profileData.role ?? 'Student'}
                fieldKey="role"
                canEdit={false}
                onEdit={handleEditClick}
              />

              {profileData.department && (
                <EditableInfoItem
                  icon={<Briefcase className="h-4 w-4" />}
                  label={t('common.department')}
                  value={profileData.department}
                  fieldKey="department"
                  canEdit={isOwnProfile}
                  onEdit={handleEditClick}
                />
              )}

              {profileData.phoneNumber && (
                <EditableInfoItem
                  icon={<Phone className="h-4 w-4" />}
                  label={t('common.phoneNumber')}
                  value={profileData.phoneNumber}
                  fieldKey="phoneNumber"
                  canEdit={isOwnProfile}
                  onEdit={handleEditClick}
                />
              )}
            </div>
          </CardContent>
        </Card>
      </div>

      {isOwnProfile && (
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
                error={editError}
              />
            </div>
          </div>
        </BaseModal>
      )}
    </div>
  )
}
