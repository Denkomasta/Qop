import { createFileRoute } from '@tanstack/react-router'
import { useAuthStore } from '@/store/useAuthStore'
import { useExtendedUserProfile } from '@/hooks/useExtendedUserProfile' // Adjust path
import { Avatar, AvatarFallback } from '@/components/ui/Avatar'
import { InfoItem } from '@/components/ui/InfoItem'
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/Card'
import {
  Mail,
  Phone,
  Briefcase,
  Shield,
  Star,
  User as UserIcon,
} from 'lucide-react'
import { useTranslation } from 'react-i18next'

export const Route = createFileRoute('/app/_authenticated/profile/')({
  component: ProfilePage,
})

function ProfilePage() {
  const { t } = useTranslation()
  const user = useAuthStore((s) => s.user)

  const { data: extendedData, isLoading } = useExtendedUserProfile(
    user?.id,
    user?.role,
  )

  if (!user) return null

  const initials = user.username.substring(0, 2).toUpperCase()

  return (
    <div className="container mx-auto max-w-7xl p-6">
      <h1 className="mb-8 text-3xl font-bold tracking-tight text-foreground">
        {t('profile.title')}
      </h1>

      <div className="grid gap-6 md:grid-cols-3">
        {/* Left Column: Avatar & Gamification Info */}
        <Card className="shadow-sm md:col-span-1">
          <CardContent className="flex flex-col items-center pt-8">
            <Avatar className="h-32 w-32 border-4 border-primary/10">
              <AvatarFallback className="bg-primary text-4xl font-bold text-primary-foreground">
                {initials}
              </AvatarFallback>
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
            <CardTitle>{t('profile.accountDetails')}</CardTitle>
            <CardDescription>{t('profile.description')}</CardDescription>
          </CardHeader>
          <CardContent className="space-y-6">
            <div className="grid gap-4 sm:grid-cols-2">
              <InfoItem
                icon={<UserIcon className="h-4 w-4" />}
                label={t('common.username')}
                value={user.username}
              />
              <InfoItem
                icon={<Mail className="h-4 w-4" />}
                label={t('common.email')}
                value={user.email}
              />
              <InfoItem
                icon={<Shield className="h-4 w-4" />}
                label={t('common.role')}
                value={user.role}
              />

              {isLoading ? (
                <p className="text-sm text-muted-foreground">
                  {t('common.loading')}...
                </p>
              ) : (
                <>
                  {extendedData &&
                    'department' in extendedData &&
                    extendedData.department && (
                      <InfoItem
                        icon={<Briefcase className="h-4 w-4" />}
                        label={t('common.department')}
                        value={extendedData.department}
                      />
                    )}
                  {extendedData &&
                    'phoneNumber' in extendedData &&
                    extendedData.phoneNumber && (
                      <InfoItem
                        icon={<Phone className="h-4 w-4" />}
                        label={t('common.phoneNumber')}
                        value={extendedData.phoneNumber}
                      />
                    )}
                </>
              )}
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
