import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { ArrowLeft } from 'lucide-react'
import { Button } from '@/components/ui/Button'
import { Spinner } from '@/components/ui/Spinner'
import { useAuthStore } from '@/store/useAuthStore'

import { useGetApiUsersIdDetails } from '@/api/generated/endpoints/user/user'
import type { BadgeDto } from '@/api/generated/model'

import { StudentBadge } from '@/components/ui/StudentBadge'
import { useGetApiBadges } from '@/api/generated/endpoints/badges/badges'
import { Link } from '@tanstack/react-router'
import { BadgeDetailsModal } from './BadgeDetailsModal'

export function BadgesView({ targetUserId }: { targetUserId?: number }) {
  const { t } = useTranslation()
  const currentUser = useAuthStore((s) => s.user)
  const idToFetch = Number(targetUserId || currentUser?.id)

  const [selectedBadge, setSelectedBadge] = useState<BadgeDto | null>(null)

  const { data: profileData, isLoading: isLoadingUser } =
    useGetApiUsersIdDetails(idToFetch, {
      query: { enabled: !!idToFetch },
    })

  const { data: badgeData, isLoading: isLoadingBadges } = useGetApiBadges({
    query: { enabled: !!idToFetch },
  })
  const allBadges: BadgeDto[] = badgeData ?? []

  if (isLoadingUser || isLoadingBadges) {
    return (
      <div className="flex min-h-[60vh] flex-col items-center justify-center gap-4">
        <Spinner size="lg" />
      </div>
    )
  }

  const earnedBadgesMap = new Map(
    profileData?.badges?.map((b) => [Number(b.badgeId), b]) || [],
  )

  const totalBadges = allBadges?.length || 0
  const earnedCount = earnedBadgesMap.size

  const isSelectedBadgeEarned = selectedBadge
    ? earnedBadgesMap.has(Number(selectedBadge.id))
    : false

  const selectedEarnedDate = selectedBadge
    ? earnedBadgesMap.get(Number(selectedBadge.id))?.earnedAt
    : undefined

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
            <button
              key={catalogBadge.id}
              onClick={() => setSelectedBadge(catalogBadge)}
              className="group flex flex-col items-center text-left focus:outline-none"
            >
              <div className="w-full transition-transform duration-200 group-hover:scale-105 group-focus:scale-105">
                <StudentBadge
                  name={catalogBadge.name}
                  iconUrl={catalogBadge.iconUrl}
                  earnedAt={userBadgeData?.earnedAt}
                  isEarned={isEarned}
                />
              </div>
            </button>
          )
        })}
      </div>

      <BadgeDetailsModal
        isOpen={!!selectedBadge}
        onClose={() => setSelectedBadge(null)}
        badge={selectedBadge}
        isEarned={isSelectedBadgeEarned}
        earnedDate={selectedEarnedDate}
      />
    </div>
  )
}
