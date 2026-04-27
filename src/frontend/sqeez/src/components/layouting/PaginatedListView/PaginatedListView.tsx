import type { ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { Search } from 'lucide-react'
import { DebouncedInput } from '@/components/ui/Input/DebouncedInput'
import { Pagination } from '@/components/ui/Pagination'
import { PageLayout } from '../PageLayout/PageLayout'

interface PaginatedListViewProps<T> {
  titleNode: ReactNode
  backButtonNode?: ReactNode
  headerActions?: ReactNode
  items: T[]
  renderItem: (item: T) => ReactNode
  totalCount: number
  emptyStateMessage: ReactNode | string
  isLoading: boolean
  isFetching?: boolean
  searchQuery?: string
  setSearchQuery?: (query: string) => void
  searchPlaceholder?: string
  pageNumber?: number
  totalPages?: number
  setPageNumber?: (page: number) => void
  filtersNode?: ReactNode
}

export function PaginatedListView<T>({
  titleNode,
  backButtonNode,
  headerActions,
  items,
  renderItem,
  totalCount,
  emptyStateMessage,
  isLoading,
  isFetching = false,
  searchQuery,
  setSearchQuery,
  searchPlaceholder,
  pageNumber,
  totalPages,
  setPageNumber,
  filtersNode,
}: PaginatedListViewProps<T>) {
  const { t } = useTranslation()

  return (
    <PageLayout
      isLoading={isLoading}
      title={
        <div className="flex items-center gap-3">
          {backButtonNode}
          {titleNode}
          <span className="text-lg font-medium text-muted-foreground">
            {t('common.totalCount', {
              count: totalCount,
              defaultValue: `(${totalCount})`,
            })}
          </span>
        </div>
      }
      headerActions={headerActions}
    >
      {(setSearchQuery || filtersNode) && (
        <div className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          {setSearchQuery && (
            <DebouncedInput
              id="list-search"
              value={searchQuery || ''}
              onChange={(newQuery) => {
                setSearchQuery(newQuery)
                if (setPageNumber) setPageNumber(1)
              }}
              placeholder={searchPlaceholder || t('common.search', 'Search...')}
              icon={<Search className="h-4 w-4" />}
              className="sm:max-w-xs"
              hideErrors
            />
          )}

          {filtersNode && <div>{filtersNode}</div>}
        </div>
      )}

      <div
        className={`transition-opacity duration-200 ${isFetching && !isLoading ? 'opacity-50' : 'opacity-100'}`}
      >
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {items.length > 0 ? (
            items.map((item) => renderItem(item))
          ) : (
            <div className="col-span-full py-12 text-center text-muted-foreground">
              {searchQuery
                ? t('common.noSearchResults', 'No results found.')
                : emptyStateMessage}
            </div>
          )}
        </div>

        {totalPages !== undefined &&
          totalPages > 1 &&
          setPageNumber &&
          pageNumber !== undefined && (
            <div className="mt-8 flex justify-center">
              <Pagination
                currentPage={pageNumber}
                totalPages={totalPages}
                onPageChange={setPageNumber}
              />
            </div>
          )}
      </div>
    </PageLayout>
  )
}
