import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Plus, Search, Edit2, Trash2, Eye, Shield, Star } from 'lucide-react'

import {
  useDeleteApiBadgesId,
  useGetApiBadges,
} from '@/api/generated/endpoints/badges/badges'
import type { BadgeDto } from '@/api/generated/model'

import { Button } from '@/components/ui/Button'
import { DebouncedInput } from '@/components/ui/Input/DebouncedInput'
import { DataTable, type ColumnDef } from '@/components/ui/Table/DataTable'
import { Pagination } from '@/components/ui/Pagination'
import { getImageUrl } from '@/lib/imageHelpers'

import { BadgeDetailsModal } from '../../../badges/-/BadgeDetailsModal'
import { ConfirmModal } from '@/components/ui'
import { CreateBadgeModal } from './CreateBadgeModal'
import { EditBadgeModal } from './EditBadgeModal'

export function AdminBadgesPage() {
  const { t } = useTranslation()

  const [searchQuery, setSearchQuery] = useState('')
  const [pageNumber, setPageNumber] = useState(1)
  const PAGE_SIZE = 15

  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false)
  const [badgeToView, setBadgeToView] = useState<BadgeDto | null>(null)
  const [badgeToEdit, setBadgeToEdit] = useState<BadgeDto | null>(null)
  const [badgeToDelete, setBadgeToDelete] = useState<BadgeDto | null>(null)

  const { data: pagedBadges, isLoading } = useGetApiBadges({
    SearchTerm: searchQuery || undefined,
    PageNumber: pageNumber,
    PageSize: PAGE_SIZE,
  })

  const deleteBadgeMutation = useDeleteApiBadgesId()

  const badges = pagedBadges?.data || []
  const totalCount = Number(pagedBadges?.totalCount || 0)
  const totalPages = Number(pagedBadges?.totalPages || 1)

  const columns: ColumnDef<BadgeDto>[] = [
    {
      header: t('admin.badges.badge'),
      cell: (badge) => (
        <div className="flex items-center gap-3">
          <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-primary/10 p-1.5">
            {badge.iconUrl ? (
              <img
                src={getImageUrl(badge.iconUrl)}
                alt={badge.name}
                className="h-full w-full object-contain"
              />
            ) : (
              <Shield className="h-5 w-5 text-primary" />
            )}
          </div>
          <div className="flex flex-col">
            <span className="font-semibold text-foreground">{badge.name}</span>
            <span className="flex items-center gap-1 text-xs font-medium text-yellow-600 dark:text-yellow-500">
              <Star className="h-3 w-3 fill-yellow-500" /> +{badge.xpBonus} XP
            </span>
          </div>
        </div>
      ),
    },
    {
      header: t('common.description'),
      cell: (badge) => (
        <span className="line-clamp-2 text-sm text-muted-foreground">
          {badge.description}
        </span>
      ),
    },
    {
      header: '',
      className: 'w-[140px] text-right',
      cell: (badge) => (
        <div className="flex justify-end gap-1">
          <Button
            variant="ghost"
            size="sm"
            className="h-8 w-8 p-0 text-muted-foreground hover:text-foreground"
            onClick={() => setBadgeToView(badge)}
            title={t('common.viewDetails')}
          >
            <Eye className="h-4 w-4" />
          </Button>
          <Button
            variant="ghost"
            size="sm"
            className="h-8 w-8 p-0 text-muted-foreground hover:text-primary"
            onClick={() => setBadgeToEdit(badge)}
            title={t('common.edit')}
          >
            <Edit2 className="h-4 w-4" />
          </Button>
          <Button
            variant="ghost"
            size="sm"
            className="h-8 w-8 p-0 text-muted-foreground hover:bg-destructive/10 hover:text-destructive"
            onClick={() => setBadgeToDelete(badge)}
            title={t('common.delete')}
          >
            <Trash2 className="h-4 w-4" />
          </Button>
        </div>
      ),
    },
  ]

  const handleDeleteConfirm = async () => {
    if (!badgeToDelete) return
    try {
      await deleteBadgeMutation.mutateAsync({ id: badgeToDelete.id.toString() })
      console.log('Deleted badge:', badgeToDelete.id)
      setBadgeToDelete(null)
    } catch (error) {
      console.error('Failed to delete badge', error)
    }
  }

  return (
    <div className="flex h-full flex-col bg-background">
      <div className="border-b border-border bg-card p-6">
        <div className="mx-auto flex max-w-7xl flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <h1 className="text-3xl font-bold tracking-tight text-foreground">
              {t('admin.badges.title')}
            </h1>
            <p className="mt-1 text-sm text-muted-foreground">
              {t('admin.badges.subtitle')}
            </p>
          </div>
          <Button onClick={() => setIsCreateModalOpen(true)} className="gap-2">
            <Plus className="h-4 w-4" />
            {t('admin.badges.createBadge')}
          </Button>
        </div>
      </div>

      <div className="flex-1 overflow-y-auto p-6">
        <div className="mx-auto max-w-7xl space-y-4">
          <div className="flex items-center justify-between gap-4">
            <DebouncedInput
              id="admin-badge-search"
              value={searchQuery}
              onChange={(val) => {
                setSearchQuery(val)
                setPageNumber(1)
              }}
              placeholder={t('admin.badges.search')}
              icon={<Search className="h-4 w-4" />}
              className="max-w-md bg-card"
              hideErrors
            />
            <span className="text-sm font-medium text-nowrap text-muted-foreground">
              {t('common.total')}: {totalCount}
            </span>
          </div>

          <div className="rounded-md border border-border bg-card">
            <DataTable
              data={badges}
              columns={columns}
              isLoading={isLoading}
              emptyMessage={t('admin.badges.noBadgesFound')}
              keyExtractor={(b) => b.id.toString()}
            />
          </div>

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
      </div>

      <BadgeDetailsModal
        isOpen={!!badgeToView}
        onClose={() => setBadgeToView(null)}
        badge={badgeToView}
        isEarned={false}
      />

      <ConfirmModal
        isOpen={!!badgeToDelete}
        onClose={() => setBadgeToDelete(null)}
        onConfirm={handleDeleteConfirm}
        title={t('admin.badges.deleteTitle')}
        description={t('admin.badges.deleteDesc', {
          badgeName: badgeToDelete?.name,
        })}
        confirmText={t('common.delete')}
        isDestructive={true}
      />

      <CreateBadgeModal
        isOpen={isCreateModalOpen}
        onClose={() => setIsCreateModalOpen(false)}
      />
      <EditBadgeModal
        isOpen={!!badgeToEdit}
        badge={badgeToEdit}
        onClose={() => setBadgeToEdit(null)}
      />
    </div>
  )
}
