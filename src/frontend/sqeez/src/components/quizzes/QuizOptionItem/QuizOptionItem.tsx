import type { StudentQuizOptionDto } from '@/api/generated/model'
import { useTranslation } from 'react-i18next'

interface QuizOptionItemProps {
  option: StudentQuizOptionDto
  isSelected: boolean
  onSelect: () => void
  mediaNode?: React.ReactNode
  freeTextValue?: string
  onFreeTextChange?: (text: string) => void
}

export function QuizOptionItem({
  option,
  isSelected,
  onSelect,
  mediaNode,
  freeTextValue = '',
  onFreeTextChange,
}: QuizOptionItemProps) {
  const { t } = useTranslation()

  if (option.isFreeText) {
    return (
      <div className="flex w-full flex-col gap-3 rounded-xl border border-border bg-card p-4 shadow-sm transition-all focus-within:border-primary focus-within:ring-1 focus-within:ring-primary">
        {(option.text || mediaNode) && (
          <div className="mb-2 flex flex-col gap-3">
            {option.text && (
              <span className="text-sm font-medium text-foreground">
                {option.text}
              </span>
            )}
            {mediaNode && (
              <div className="w-full overflow-hidden rounded-lg border sm:w-64">
                {mediaNode}
              </div>
            )}
          </div>
        )}

        <textarea
          className="min-h-30 w-full resize-y rounded-md border border-input bg-background px-3 py-2 text-base ring-offset-background placeholder:text-muted-foreground focus-visible:ring-2 focus-visible:ring-ring focus-visible:outline-none disabled:cursor-not-allowed disabled:opacity-50"
          placeholder={t('quiz.typeAnswerHere', 'Type your answer here...')}
          value={freeTextValue}
          onChange={(e) => onFreeTextChange?.(e.target.value)}
          onFocus={onSelect}
        />
      </div>
    )
  }

  return (
    <button
      type="button"
      onClick={onSelect}
      className={`flex w-full flex-col gap-4 rounded-xl border p-4 text-left transition-all focus:ring-2 focus:ring-primary focus:ring-offset-2 focus:outline-none sm:flex-row sm:items-center ${
        isSelected
          ? 'border-primary bg-primary/5 shadow-sm ring-1 ring-primary'
          : 'border-border bg-card hover:border-primary/50 hover:bg-muted/50'
      }`}
    >
      <div className="flex w-full items-center gap-4 sm:w-auto sm:flex-1">
        <div
          className={`flex size-5 shrink-0 items-center justify-center rounded-full border ${
            isSelected ? 'border-primary bg-primary' : 'border-muted-foreground'
          }`}
        >
          {isSelected && (
            <div className="size-2 rounded-full bg-primary-foreground" />
          )}
        </div>

        <span
          className={`text-base ${
            isSelected ? 'font-medium text-foreground' : 'text-muted-foreground'
          }`}
        >
          {option.text}
        </span>
      </div>

      {mediaNode && (
        <div className="mt-2 w-full shrink-0 overflow-hidden rounded-lg border sm:mt-0 sm:w-32 md:w-48">
          {mediaNode}
        </div>
      )}
    </button>
  )
}
