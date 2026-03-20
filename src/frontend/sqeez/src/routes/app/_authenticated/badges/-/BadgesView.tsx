import { useState, useMemo } from 'react'
import { useTranslation } from 'react-i18next'
import { ArrowLeft, Search } from 'lucide-react'
import { Button } from '@/components/ui/Button'
import { Spinner } from '@/components/ui/Spinner'
import { useAuthStore } from '@/store/useAuthStore'

import { StudentBadge } from '@/components/ui/StudentBadge'
import type { BadgeDto } from '@/api/generated/model'
import { Link } from '@tanstack/react-router'
import {
  useGetApiBadges,
  useGetApiBadgesStudentStudentId,
} from '@/api/generated/endpoints/badges/badges'
import { BadgeDetailsModal } from './BadgeDetailsModal'
import { DebouncedInput } from '@/components/ui/Input/DebouncedInput'
import { Pagination } from '@/components/ui/Pagination'

export function BadgesView({ targetUserId }: { targetUserId?: number }) {
  const { t } = useTranslation()
  const currentUser = useAuthStore((s) => s.user)

  const idToFetch = Number(targetUserId || currentUser?.id)

  const [selectedBadge, setSelectedBadge] = useState<BadgeDto | null>(null)
  const [searchQuery, setSearchQuery] = useState('')
  const [filterStatus, setFilterStatus] = useState<'all' | 'earned' | 'locked'>(
    'all',
  )
  const [pageNumber, setPageNumber] = useState(1)
  const PAGE_SIZE = 12

  const { data: earnedBadges, isLoading: isLoadingEarned } =
    useGetApiBadgesStudentStudentId(idToFetch, {
      query: { enabled: !!idToFetch },
    })

  const {
    data: pagedCatalog,
    isLoading: isLoadingCatalog,
    isFetching: isFetchingCatalog,
  } = useGetApiBadges({
    SearchTerm: searchQuery,
    PageNumber: pageNumber,
    PageSize: PAGE_SIZE,
    isEarned: filterStatus == 'all' ? undefined : filterStatus == 'earned',
    StudentId: idToFetch,
  })

  const earnedBadgesMap = useMemo(() => {
    return new Map(earnedBadges?.map((b) => [Number(b.badgeId), b]) || [])
  }, [earnedBadges])

  if (isLoadingEarned) {
    return (
      <div className="flex min-h-[60vh] flex-col items-center justify-center gap-4">
        <Spinner size="lg" />
      </div>
    )
  }

  const displayBadges = pagedCatalog?.data || []
  const totalBadges = Number(pagedCatalog?.totalCount || 0)
  const totalPages = Number(
    pagedCatalog?.totalPages || Math.ceil(totalBadges / PAGE_SIZE),
  )
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

      <div className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <DebouncedInput
          id="badge-search"
          value={searchQuery}
          onChange={(newQuery) => {
            setSearchQuery(newQuery)
            setPageNumber(1)
          }}
          placeholder={t('badges.search', 'Search badges...')}
          icon={<Search className="h-4 w-4" />}
          className="sm:max-w-xs"
        />

        <div className="flex w-full items-center rounded-lg border bg-muted/50 p-1 sm:w-auto">
          {(['all', 'earned', 'locked'] as const).map((status) => (
            <button
              key={status}
              onClick={() => setFilterStatus(status)}
              className={`flex-1 rounded-md px-4 py-1.5 text-sm font-medium transition-colors sm:flex-none ${
                filterStatus === status
                  ? 'bg-background text-foreground shadow-sm'
                  : 'text-muted-foreground hover:text-foreground'
              }`}
            >
              {t(
                `badges.filter.${status}`,
                status.charAt(0).toUpperCase() + status.slice(1),
              )}
            </button>
          ))}
        </div>
      </div>

      <div
        className={`transition-opacity duration-200 ${isFetchingCatalog && !isLoadingCatalog ? 'opacity-50' : 'opacity-100'}`}
      >
        {isLoadingCatalog ? (
          <div className="flex min-h-[40vh] items-center justify-center">
            <Spinner size="lg" />
          </div>
        ) : (
          <>
            <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6">
              {displayBadges?.length > 0 ? (
                displayBadges.map((catalogBadge) => {
                  const userBadgeData = earnedBadgesMap.get(
                    Number(catalogBadge.id),
                  )
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
                })
              ) : (
                <div className="col-span-full py-12 text-center text-muted-foreground">
                  {t('badges.noResults', 'No badges found.')}
                </div>
              )}
            </div>

            <div className="mt-6">
              <Pagination
                currentPage={pageNumber}
                totalPages={totalPages}
                onPageChange={setPageNumber}
              />
            </div>
          </>
        )}
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
