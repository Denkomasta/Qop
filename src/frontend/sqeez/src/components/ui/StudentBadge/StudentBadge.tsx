import { Shield } from 'lucide-react'
import { getImageUrl } from '@/lib/imageHelpers'

interface StudentBadgeProps {
  name?: string
  iconUrl?: string | null
  earnedAt?: string
}

export function StudentBadge({ name, iconUrl, earnedAt }: StudentBadgeProps) {
  return (
    <div className="flex flex-col items-center gap-3 rounded-lg border bg-card p-4 text-center shadow-sm transition-colors hover:bg-muted/50">
      <div className="flex h-16 w-16 shrink-0 items-center justify-center rounded-full bg-primary/10 p-2">
        {iconUrl ? (
          <img
            src={getImageUrl(iconUrl)}
            alt={name}
            className="h-full w-full object-contain"
          />
        ) : (
          <Shield className="h-8 w-8 text-primary" />
        )}
      </div>
      <div className="flex flex-col gap-1">
        <span className="line-clamp-2 text-sm leading-tight font-semibold text-foreground">
          {name}
        </span>
        {earnedAt && (
          <span className="text-xs text-muted-foreground">
            {new Date(earnedAt).toLocaleDateString()}
          </span>
        )}
      </div>
    </div>
  )
}
