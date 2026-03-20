import { useTranslation } from 'react-i18next'
import { ArrowLeft } from 'lucide-react'
import { Button } from '@/components/ui/Button'
import { Spinner } from '@/components/ui/Spinner'
import { useAuthStore } from '@/store/useAuthStore'

import { useGetApiUsersIdDetails } from '@/api/generated/endpoints/user/user'
import { StudentBadge } from '@/components/ui/StudentBadge'
import { Link } from '@tanstack/react-router'

export function BadgesView({ targetUserId }: { targetUserId?: number }) {
  const { t } = useTranslation()
  const currentUser = useAuthStore((s) => s.user)

  const idToFetch = targetUserId || currentUser?.id

  const { data: profileData, isLoading: isLoadingUser } =
    useGetApiUsersIdDetails(idToFetch!, {
      query: { enabled: !!idToFetch },
    })

  const isLoadingBadges = false
  const allBadges = [
    { id: 1, name: 'First Login', iconUrl: null },
    { id: 2, name: 'Homework Hero', iconUrl: null },
    { id: 3, name: 'Perfect Attendance', iconUrl: null },
  ]

  if (isLoadingUser || isLoadingBadges) {
    return (
      <div className="flex min-h-[60vh] flex-col items-center justify-center gap-4">
        <Spinner size="lg" />
        <p className="animate-pulse font-medium text-muted-foreground">
          {t('common.loading')}...
        </p>
      </div>
    )
  }

  const earnedBadgesMap = new Map(
    profileData?.badges?.map((b) => [Number(b.badgeId), b]) || [],
  )

  const totalBadges = allBadges?.length || 0
  const earnedCount = earnedBadgesMap.size

  return (
    <div className="container mx-auto max-w-7xl p-6">
      <div className="mb-8 flex flex-col gap-4">
        <div>
          <Button variant="ghost" size="sm" asChild className="mb-4 -ml-3">
            <Link
              to="/app/profile/$userId"
              params={{ userId: (idToFetch ?? 0).toString() }}
            >
              <ArrowLeft className="mr-2 h-4 w-4" />
              {t('common.backToProfile', 'Back to Profile')}
            </Link>
          </Button>
          <div className="flex flex-col gap-2 sm:flex-row sm:items-end sm:justify-between">
            <h1 className="text-3xl font-bold tracking-tight text-foreground">
              {t('badges.title', 'All Badges')}
            </h1>
            <p className="font-medium text-muted-foreground">
              {earnedCount} / {totalBadges} {t('badges.earned', 'Earned')}
            </p>
          </div>
        </div>

        <div className="h-2 w-full overflow-hidden rounded-full bg-secondary">
          <div
            className="h-full bg-primary transition-all duration-500"
            style={{
              width: `${totalBadges > 0 ? (earnedCount / totalBadges) * 100 : 0}%`,
            }}
          />
        </div>
      </div>

      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6">
        {allBadges?.map((catalogBadge) => {
          const userBadgeData = earnedBadgesMap.get(Number(catalogBadge.id))
          const isEarned = !!userBadgeData

          return (
            <StudentBadge
              key={catalogBadge.id}
              name={catalogBadge.name}
              iconUrl={catalogBadge.iconUrl}
              earnedAt={userBadgeData?.earnedAt}
              isEarned={isEarned}
            />
          )
        })}
      </div>
    </div>
  )
}
