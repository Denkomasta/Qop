import { Button } from '@/components/ui/Button'
import { InfoItem } from '@/components/ui/InfoItem'
import { Pencil } from 'lucide-react'

interface EditableInfoItemProps {
  icon: React.ReactNode
  label: string
  value: string | number
  fieldKey: string
  canEdit?: boolean
  buttonText?: string
  onEdit: (key: string, label: string, value: string) => void
}

export function EditableInfoItem({
  icon,
  label,
  value,
  fieldKey,
  canEdit = true,
  buttonText,
  onEdit,
}: EditableInfoItemProps) {
  return (
    <div className="group relative flex h-full items-center justify-between rounded-lg border border-transparent p-2 transition-colors focus-within:border-border focus-within:bg-muted/50 hover:border-border hover:bg-muted/50">
      <div className="min-w-0 flex-1">
        <InfoItem icon={icon} label={label} value={String(value)} />
      </div>

      <div className="ml-2 flex h-9 w-9 shrink-0 items-center justify-center">
        {canEdit && (
          <Button
            variant="ghost"
            size="icon"
            className="opacity-0 transition-opacity group-hover:opacity-100 focus:opacity-100"
            onClick={() => onEdit(fieldKey, label, String(value))}
            title={`${buttonText} ${label}`}
          >
            <Pencil className="h-4 w-4 text-muted-foreground" />
          </Button>
        )}
      </div>
    </div>
  )
}
