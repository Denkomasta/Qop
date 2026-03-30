import { type ReactNode } from 'react'
import { cn } from '@/lib/utils'

interface FeatureCardProps {
  icon: ReactNode
  iconWrapperClassName?: string
  title: string
  description: string
}

export function FeatureCard({
  icon,
  iconWrapperClassName,
  title,
  description,
}: FeatureCardProps) {
  return (
    <div className="flex flex-col items-center rounded-2xl border border-border bg-card p-8 text-center shadow-sm transition-shadow hover:shadow-md">
      <div
        className={cn(
          'mb-6 flex h-14 w-14 items-center justify-center rounded-full',
          iconWrapperClassName,
        )}
      >
        {icon}
      </div>
      <h3 className="mb-3 text-xl font-bold text-foreground">{title}</h3>
      <p className="text-muted-foreground">{description}</p>
    </div>
  )
}
