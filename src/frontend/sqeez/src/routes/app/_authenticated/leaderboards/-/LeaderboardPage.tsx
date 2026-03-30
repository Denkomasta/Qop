import { useState } from 'react'
import { Link } from '@tanstack/react-router'
import { useTranslation } from 'react-i18next'
import { Trophy, Medal, Search } from 'lucide-react'
import { DebouncedInput } from '@/components/ui/Input/DebouncedInput'
import { Pagination } from '@/components/ui/Pagination'
import { SimpleAvatar } from '@/components/ui/Avatar'
import { Badge } from '@/components/ui/Badge/Badge'
import { getImageUrl } from '@/lib/imageHelpers'
import { formatName } from '@/lib/userHelpers'
import { useAuthStore } from '@/store/useAuthStore'
import { cn } from '@/lib/utils'
import { useGetApiUsers } from '@/api/generated/endpoints/user/user'

export function LeaderboardPage() {
  const { t } = useTranslation()
  const user = useAuthStore((s) => s.user)

  const [searchQuery, setSearchQuery] = useState('')
  const [pageNumber, setPageNumber] = useState(1)
  const pageSize = 10

  const { data: usersResponse, isLoading } = useGetApiUsers({
    Role: 'Student',
    SearchTerm: searchQuery || undefined,
    PageNumber: pageNumber,
    PageSize: pageSize,
    StrictRoleOnly: true,
    SortBy: 'XP',
    IsDescending: true,
  })

  const students = usersResponse?.data || []
  const totalPages = Number(usersResponse?.totalPages || 1)

  const getRankBadge = (globalRank: number) => {
    switch (globalRank) {
      case 1:
        return <Trophy className="h-6 w-6 text-yellow-500 drop-shadow-sm" />
      case 2:
        return <Medal className="h-6 w-6 text-slate-400 drop-shadow-sm" />
      case 3:
        return <Medal className="h-6 w-6 text-amber-700 drop-shadow-sm" />
      default:
        return (
          <span className="text-lg font-bold text-muted-foreground">
            {globalRank}
          </span>
        )
    }
  }

  return (
    <div className="flex h-full flex-col bg-background">
      <div className="border-b border-border bg-card p-6 md:p-8">
        <div className="mx-auto flex max-w-4xl flex-col gap-6">
          <div className="flex items-center gap-3">
            <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-primary/10 text-primary">
              <Trophy className="h-6 w-6" />
            </div>
            <div>
              <h1 className="text-3xl font-bold tracking-tight">
                {t('leaderboard.title', 'Leaderboard')}
              </h1>
              <p className="text-muted-foreground">
                {t('leaderboard.subtitle', 'Top performing students by XP')}
              </p>
            </div>
          </div>

          <div className="flex w-full max-w-sm items-center gap-4">
            <DebouncedInput
              id="leaderboard-search"
              value={searchQuery}
              onChange={(newQuery) => {
                setSearchQuery(newQuery)
                setPageNumber(1)
              }}
              placeholder={t(
                'leaderboard.searchPlaceholder',
                'Search students...',
              )}
              icon={<Search className="h-4 w-4" />}
              hideErrors
            />
          </div>
        </div>
      </div>

      <div className="flex-1 overflow-y-auto p-6 md:p-8">
        <div className="mx-auto max-w-4xl">
          {isLoading ? (
            <div className="flex flex-col gap-3">
              {Array.from({ length: 5 }).map((_, i) => (
                <div
                  key={i}
                  className="h-20 w-full animate-pulse rounded-2xl bg-muted/50"
                />
              ))}
            </div>
          ) : students.length === 0 ? (
            <div className="flex flex-col items-center justify-center rounded-2xl border-2 border-dashed border-muted p-12 text-center">
              <Trophy className="mb-4 h-12 w-12 text-muted-foreground/30" />
              <h3 className="text-lg font-semibold">
                {t('leaderboard.noStudentsFound', 'No students found')}
              </h3>
              <p className="text-sm text-muted-foreground">
                {t(
                  'leaderboard.tryDifferentSearch',
                  'Try adjusting your search filters.',
                )}
              </p>
            </div>
          ) : (
            <div className="flex flex-col gap-3">
              {students.map((student, index) => {
                const globalRank = (pageNumber - 1) * pageSize + index + 1
                const isTopThree = globalRank <= 3
                const isMe = student.id === user?.id

                return (
                  <div
                    key={student.id}
                    className={cn(
                      'flex items-center justify-between rounded-2xl border p-4 transition-all hover:shadow-md sm:px-6',
                      isMe
                        ? 'border-primary bg-primary/10 shadow-sm ring-1 ring-primary/30'
                        : isTopThree
                          ? 'border-primary/20 bg-primary/5 shadow-sm'
                          : 'border-border bg-card',
                    )}
                  >
                    <div className="flex items-center gap-4 sm:gap-6">
                      <div className="flex w-8 shrink-0 items-center justify-center">
                        {getRankBadge(globalRank)}
                      </div>

                      <Link
                        to="/app/profile/$userId"
                        params={{ userId: (student.id ?? 0).toString() }}
                        className="shrink-0 transition-opacity hover:opacity-80"
                      >
                        <SimpleAvatar
                          url={getImageUrl(student.avatarUrl)}
                          firstName={student.firstName}
                          lastName={student.lastName}
                          wrapperClassName="size-12"
                        />
                      </Link>

                      <div className="flex flex-col">
                        <span className="flex items-center gap-2 font-bold text-foreground sm:text-lg">
                          <Link
                            to="/app/profile/$userId"
                            params={{ userId: (student.id ?? 0).toString() }}
                            className="hover:underline"
                          >
                            {student.username}
                          </Link>
                          {isMe && (
                            <Badge
                              variant="default"
                              className="h-5 px-1.5 text-[10px]"
                            >
                              {t('class.me', 'Me')}
                            </Badge>
                          )}
                        </span>
                        <span className="text-xs text-muted-foreground sm:text-sm">
                          {formatName(student.firstName, student.lastName)}
                        </span>
                      </div>
                    </div>

                    <div className="ml-4 flex shrink-0 flex-col items-end">
                      <span className="text-lg font-black text-primary sm:text-2xl">
                        {student.currentXP || 0}
                      </span>
                      <span className="text-[10px] font-bold tracking-widest text-muted-foreground uppercase sm:text-xs">
                        XP
                      </span>
                    </div>
                  </div>
                )
              })}
            </div>
          )}

          {!isLoading && totalPages > 1 && (
            <div className="mt-8 flex justify-center">
              <Pagination
                currentPage={pageNumber}
                totalPages={totalPages}
                onPageChange={setPageNumber}
              />
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
