import { useState } from 'react'
import { Link } from '@tanstack/react-router'
import { useTranslation } from 'react-i18next'
import { Trophy, Medal } from 'lucide-react'
import { SimpleAvatar } from '@/components/ui/Avatar'
import { Badge } from '@/components/ui/Badge/Badge'
import { getImageUrl } from '@/lib/imageHelpers'
import { formatName } from '@/lib/userHelpers'
import { useAuthStore } from '@/store/useAuthStore'
import { cn } from '@/lib/utils'
import { useGetApiUsers } from '@/api/generated/endpoints/user/user'
import { PaginatedListView } from '@/components/layouting/PaginatedListView/PaginatedListView'
import type { StudentDto } from '@/api/generated/model'

export function LeaderboardPage() {
  const { t } = useTranslation()
  const user = useAuthStore((s) => s.user)

  const [searchQuery, setSearchQuery] = useState('')
  const [pageNumber, setPageNumber] = useState(1)
  const pageSize = 10

  const {
    data: usersResponse,
    isLoading,
    isFetching,
  } = useGetApiUsers(
    {
      Role: 'Student',
      SearchTerm: searchQuery || undefined,
      PageNumber: pageNumber,
      PageSize: pageSize,
      StrictRoleOnly: true,
      SortBy: 'XP',
      IsDescending: true,
    },
    {
      query: {
        placeholderData: (prev) => prev,
      },
    },
  )

  const students = usersResponse?.data || []
  const totalPages = Number(usersResponse?.totalPages || 1)
  const totalCount = Number(usersResponse?.totalCount || 0)

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
    <PaginatedListView<StudentDto>
      layoutVariant="app"
      containerClassName="max-w-4xl"
      gridClassName="flex flex-col gap-3"
      titleNode={
        <div className="flex items-center gap-3">
          <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-primary/10 text-primary">
            <Trophy className="h-6 w-6" />
          </div>
          <div>
            <h1 className="text-3xl font-bold tracking-tight">
              {t('leaderboard.title')}
            </h1>
            <p className="mt-1 text-sm font-normal text-muted-foreground">
              {t('leaderboard.subtitle')}
            </p>
          </div>
        </div>
      }
      searchQuery={searchQuery}
      setSearchQuery={setSearchQuery}
      searchPlaceholder={t('leaderboard.searchPlaceholder')}
      pageNumber={pageNumber}
      totalPages={totalPages}
      setPageNumber={setPageNumber}
      isLoading={isLoading && !usersResponse}
      isFetching={isFetching}
      items={students}
      totalCount={totalCount}
      emptyStateMessage={
        <div className="mx-auto flex max-w-4xl flex-col items-center justify-center rounded-2xl border-2 border-dashed border-muted p-12 text-center">
          <Trophy className="mb-4 h-12 w-12 text-muted-foreground/30" />
          <h3 className="text-lg font-semibold">
            {t('leaderboard.noStudentsFound')}
          </h3>
          <p className="mt-2 text-sm text-muted-foreground">
            {t('leaderboard.tryDifferentSearch')}
          </p>
        </div>
      }
      renderItem={(student, index) => {
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
                    <Badge variant="default" className="h-5 px-1.5 text-[10px]">
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
      }}
    />
  )
}
