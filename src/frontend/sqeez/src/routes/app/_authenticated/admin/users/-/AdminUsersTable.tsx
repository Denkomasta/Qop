import { useTranslation } from 'react-i18next'
import { Link } from '@tanstack/react-router'
import { ShieldAlert, GraduationCap, BookOpen, Edit2 } from 'lucide-react'

import { SimpleAvatar } from '@/components/ui/Avatar'
import { Badge } from '@/components/ui/Badge/Badge'
import { getImageUrl } from '@/lib/imageHelpers'
import { formatName } from '@/lib/userHelpers'
import type { StudentDto, UserRole } from '@/api/generated/model'

import { DataTable, type ColumnDef } from '@/components/ui/Table/DataTable'

export interface SelectedUserForRole {
  id: string | number
  name: string
  currentRole: UserRole
}

interface AdminUsersTableProps {
  users: StudentDto[]
  isLoading: boolean
  onEditRole: (user: SelectedUserForRole) => void
}

export function AdminUsersTable({
  users,
  isLoading,
  onEditRole,
}: AdminUsersTableProps) {
  const { t } = useTranslation()

  const getRoleBadge = (role?: UserRole) => {
    switch (role) {
      case 'Admin':
        return (
          <Badge className="bg-destructive hover:bg-destructive/80">
            <ShieldAlert className="mr-1 h-3 w-3" /> Admin
          </Badge>
        )
      case 'Teacher':
        return (
          <Badge className="bg-blue-600 hover:bg-blue-700">
            <BookOpen className="mr-1 h-3 w-3" /> Teacher
          </Badge>
        )
      case 'Student':
      default:
        return (
          <Badge variant="secondary">
            <GraduationCap className="mr-1 h-3 w-3" /> Student
          </Badge>
        )
    }
  }

  const columns: ColumnDef<StudentDto>[] = [
    {
      header: t('common.user'),
      cell: (user) => (
        <div className="flex items-center gap-3">
          <SimpleAvatar
            url={getImageUrl(user.avatarUrl)}
            firstName={user.firstName}
            lastName={user.lastName}
            wrapperClassName="size-10 shrink-0"
          />
          <div className="flex flex-col">
            <Link
              to="/app/profile/$userId"
              params={{ userId: (user.id ?? 0).toString() }}
              className="font-semibold text-foreground hover:underline"
            >
              {formatName(user.firstName, user.lastName)}
            </Link>
            <span className="text-xs text-muted-foreground">
              @{user.username}
            </span>
          </div>
        </div>
      ),
    },
    {
      header: t('common.contact'),
      cell: (user) => (
        <span className="text-muted-foreground">{user.email}</span>
      ),
    },
    {
      header: t('common.role'),
      cell: (user) => (
        <button
          onClick={() =>
            onEditRole({
              id: user.id!,
              name: formatName(user.firstName, user.lastName),
              currentRole: user.role!,
            })
          }
          className="group relative flex items-center gap-2 rounded-md transition-all outline-none focus-visible:ring-2 focus-visible:ring-primary"
          title={t('admin.clickToEditRole')}
        >
          {getRoleBadge(user.role)}
          <Edit2 className="h-3 w-3 text-muted-foreground opacity-0 transition-opacity group-hover:opacity-100" />
        </button>
      ),
    },
  ]

  return (
    <DataTable
      data={users}
      columns={columns}
      isLoading={isLoading}
      emptyMessage={t('admin.noUsersFound')}
      keyExtractor={(user) => user?.id ?? Math.random().toString()}
    />
  )
}
