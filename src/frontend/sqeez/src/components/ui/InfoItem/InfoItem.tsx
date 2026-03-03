interface InfoItemProps {
  icon: React.ReactNode
  label: string
  value: string
  isEmpty?: boolean
}

export function InfoItem({
  icon,
  label,
  value,
  isEmpty = false,
}: InfoItemProps) {
  return (
    <div className="flex items-start gap-3 rounded-lg border bg-card p-4 shadow-sm transition-colors hover:bg-muted/50">
      <div className="mt-0.5 flex h-8 w-8 shrink-0 items-center justify-center rounded-md bg-primary/10 text-primary">
        {icon}
      </div>
      <div className="flex flex-col">
        <span className="text-xs font-medium tracking-wider text-muted-foreground uppercase">
          {label}
        </span>
        <span
          className={`mt-1 text-sm font-semibold ${
            isEmpty
              ? 'font-normal text-muted-foreground/70 italic'
              : 'text-foreground'
          }`}
        >
          {value}
        </span>
      </div>
    </div>
  )
}
