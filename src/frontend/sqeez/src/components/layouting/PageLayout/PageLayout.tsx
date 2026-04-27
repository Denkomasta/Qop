import { useTranslation } from 'react-i18next'
import { Spinner } from '@/components/ui/Spinner'
import type { ReactNode } from 'react'

export interface PageLayoutProps {
  title?: ReactNode
  subtitle?: ReactNode
  headerActions?: ReactNode
  isLoading?: boolean
  children: ReactNode
}

export function PageLayout({
  title,
  subtitle,
  headerActions,
  isLoading,
  children,
}: PageLayoutProps) {
  const { t } = useTranslation()

  if (isLoading) {
    return (
      <div className="flex min-h-[60vh] flex-col items-center justify-center gap-4">
        <Spinner size="lg" />
        <p className="animate-pulse font-medium text-muted-foreground">
          {t('common.loading', 'Loading')}...
        </p>
      </div>
    )
  }

  return (
    <div className="container mx-auto max-w-7xl space-y-8 p-6">
      {(title || subtitle || headerActions) && (
        <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div>
            {title && (
              <h1 className="flex items-center gap-3 text-3xl font-bold tracking-tight text-foreground">
                {title}
              </h1>
            )}
            {subtitle && (
              <p className="mt-1 text-muted-foreground">{subtitle}</p>
            )}
          </div>

          {headerActions && <div>{headerActions}</div>}
        </div>
      )}

      <main>{children}</main>
    </div>
  )
}
