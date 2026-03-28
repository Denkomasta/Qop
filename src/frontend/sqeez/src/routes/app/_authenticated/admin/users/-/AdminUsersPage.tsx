import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Users, Search } from 'lucide-react'

import { DebouncedInput } from '@/components/ui/Input/DebouncedInput'
import { Pagination } from '@/components/ui/Pagination'
import type { UserRole } from '@/api/generated/model'
import { useGetApiUsers } from '@/api/generated/endpoints/user/user'

import { RoleModificationModal } from './RoleModificationModal'
import { AdminUsersTable, type SelectedUserForRole } from './AdminUsersTable'

export function AdminUsersPage() {
  const { t } = useTranslation()

  const [searchQuery, setSearchQuery] = useState('')
  const [roleFilter, setRoleFilter] = useState<UserRole | ''>('')
  const [pageNumber, setPageNumber] = useState(1)
  const pageSize = 15

  const [selectedUserForRole, setSelectedUserForRole] =
    useState<SelectedUserForRole | null>(null)

  const { data: usersResponse, isLoading } = useGetApiUsers({
    SearchTerm: searchQuery || undefined,
    Role: roleFilter || undefined,
    StrictRoleOnly: !!roleFilter,
    PageNumber: pageNumber,
    PageSize: pageSize,
  })

  const users = usersResponse?.data || []
  const totalPages = Number(usersResponse?.totalPages || 1)
  const totalCount = usersResponse?.totalCount || 0

  return (
    <div className="flex h-full flex-col bg-background">
      <header className="border-b border-border bg-card p-6">
        <div className="mx-auto flex max-w-7xl flex-col gap-6">
          <div className="flex items-center gap-3">
            <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-primary/10 text-primary">
              <Users className="h-6 w-6" />
            </div>
            <div>
              <h1 className="text-3xl font-bold tracking-tight">
                {t('admin.userManagement')}
              </h1>
              <p className="text-muted-foreground">
                {t('admin.totalUsers')}:{' '}
                <span className="font-bold">{totalCount}</span>
              </p>
            </div>
          </div>

          <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
            <DebouncedInput
              id="admin-user-search"
              value={searchQuery}
              onChange={(val) => {
                setSearchQuery(val)
                setPageNumber(1)
              }}
              placeholder={t('admin.searchUsers')}
              icon={<Search className="h-4 w-4" />}
              className="max-w-md bg-background"
              hideErrors
            />

            <select
              className="h-10 rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background focus-visible:ring-2 focus-visible:ring-ring focus-visible:outline-none"
              value={roleFilter}
              onChange={(e) => {
                setRoleFilter(e.target.value as UserRole | '')
                setPageNumber(1)
              }}
            >
              <option value="">{t('admin.allRoles')}</option>
              <option value="Student">{t('common.students')}</option>
              <option value="Teacher">{t('common.teachers')}</option>
              <option value="Admin">{t('common.admins')}</option>
            </select>
          </div>
        </div>
      </header>

      <main className="flex-1 overflow-y-auto p-6">
        <div className="mx-auto max-w-7xl">
          <AdminUsersTable
            users={users}
            isLoading={isLoading}
            onEditRole={setSelectedUserForRole}
          />

          {!isLoading && totalPages > 1 && (
            <div className="mt-6 flex justify-center">
              <Pagination
                currentPage={pageNumber}
                totalPages={totalPages}
                onPageChange={setPageNumber}
              />
            </div>
          )}
        </div>
      </main>

      <RoleModificationModal
        key={selectedUserForRole?.id ?? 'empty-modal'}
        isOpen={!!selectedUserForRole}
        onClose={() => setSelectedUserForRole(null)}
        user={selectedUserForRole}
      />
    </div>
  )
}
