import { useTranslation } from 'react-i18next'
import { ChevronLeft, ChevronRight } from 'lucide-react'
import { Button } from '@/components/ui/Button'

interface PaginationProps {
  currentPage: number
  totalPages: number
  onPageChange: (page: number) => void
  className?: string
}

export function Pagination({
  currentPage,
  totalPages,
  onPageChange,
  className = '',
}: PaginationProps) {
  const { t } = useTranslation()

  if (totalPages <= 1) return null

  return (
    <div className={`flex items-center justify-between px-2 py-4 ${className}`}>
      <Button
        variant="outline"
        size="sm"
        onClick={() => onPageChange(currentPage - 1)}
        disabled={currentPage <= 1}
        className="min-w-25"
      >
        <ChevronLeft className="mr-2 size-4" />
        {t('common.previous', 'Previous')}
      </Button>

      <div className="text-sm font-medium text-muted-foreground">
        {t('common.page', 'Page')} {currentPage} {t('common.of', 'of')}{' '}
        {totalPages}
      </div>

      <Button
        variant="outline"
        size="sm"
        onClick={() => onPageChange(currentPage + 1)}
        disabled={currentPage >= totalPages}
        className="min-w-25"
      >
        {t('common.next', 'Next')}
        <ChevronRight className="ml-2 size-4" />
      </Button>
    </div>
  )
}
