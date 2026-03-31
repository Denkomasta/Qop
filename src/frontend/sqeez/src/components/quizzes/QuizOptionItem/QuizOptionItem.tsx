import type { StudentQuizOptionDto } from '@/api/generated/model'
import { useTranslation } from 'react-i18next'
import { Check } from 'lucide-react'
import { cn } from '@/lib/utils'
import { TextArea } from '@/components/ui/TextArea'

interface QuizOptionItemProps {
  option: StudentQuizOptionDto
  index?: number
  isSelected: boolean
  isMultiChoice?: boolean
  onSelect: () => void
  mediaNode?: React.ReactNode
  freeTextValue?: string
  onFreeTextChange?: (text: string) => void
}

const FUN_COLORS = [
  'bg-rose-500 border-rose-700 text-white hover:bg-rose-400',
  'bg-blue-500 border-blue-700 text-white hover:bg-blue-400',
  'bg-amber-400 border-amber-600 text-slate-900 hover:bg-amber-300',
  'bg-emerald-500 border-emerald-700 text-white hover:bg-emerald-400',
]

export function QuizOptionItem({
  option,
  index = 0,
  isSelected,
  isMultiChoice = false,
  onSelect,
  mediaNode,
  freeTextValue = '',
  onFreeTextChange,
}: QuizOptionItemProps) {
  const { t } = useTranslation()

  const colorTheme = FUN_COLORS[index % FUN_COLORS.length]

  if (option.isFreeText) {
    return (
      <div className="col-span-1 flex w-full flex-col gap-4 rounded-3xl border-4 border-b-8 border-primary/20 bg-card p-6 shadow-sm transition-colors focus-within:border-primary sm:col-span-2">
        {(option.text || mediaNode) && (
          <div className="mb-4 flex flex-col gap-3 text-center">
            {option.text && (
              <span className="text-xl font-bold text-foreground md:text-2xl">
                {option.text}
              </span>
            )}
            {mediaNode && (
              <div className="mx-auto w-full max-w-sm overflow-hidden rounded-2xl border-4">
                {mediaNode}
              </div>
            )}
          </div>
        )}

        <TextArea
          placeholder={t('quiz.typeAnswerHere', 'Type your answer here...')}
          value={freeTextValue}
          onChange={(e) => onFreeTextChange?.(e.target.value)}
          onFocus={onSelect}
          hideErrors
          maxLength={200}
          className="min-h-32 resize-none text-center text-lg font-bold shadow-none focus-visible:ring-2 focus-visible:ring-primary/20 md:text-xl lg:text-2xl"
        />
      </div>
    )
  }

  return (
    <button
      type="button"
      onClick={onSelect}
      className={cn(
        'group relative flex min-h-32 w-full flex-col items-center justify-center gap-4 rounded-3xl border-4 p-6 text-center transition-all focus:ring-4 focus:ring-primary/30 focus:outline-none sm:min-h-48',
        colorTheme,
        isSelected
          ? 'translate-y-2 border-b-4 shadow-[inset_0_4px_8px_rgba(0,0,0,0.15)] brightness-95'
          : 'border-b-8 hover:-translate-y-1 active:translate-y-2 active:border-b-4',
      )}
    >
      <div
        className={cn(
          'absolute top-4 right-4 flex size-8 shrink-0 items-center justify-center border-2 transition-all',
          isMultiChoice ? 'rounded-lg' : 'rounded-full',
          isSelected
            ? 'border-current bg-current'
            : 'border-current/40 bg-black/10',
        )}
      >
        {isSelected &&
          (isMultiChoice ? (
            <Check
              className="size-5 stroke-4 text-background opacity-90"
              style={{ color: 'inherit', mixBlendMode: 'difference' }}
            />
          ) : (
            <div
              className="size-3.5 rounded-full"
              style={{ backgroundColor: 'inherit', mixBlendMode: 'difference' }}
            />
          ))}
      </div>

      {mediaNode && (
        <div className="mt-2 w-full max-w-50 shrink-0 overflow-hidden rounded-xl border-4 border-foreground/10 shadow-sm">
          {mediaNode}
        </div>
      )}

      <span className="w-full text-2xl font-black tracking-wide break-words text-current md:text-3xl lg:text-4xl">
        {option.text}
      </span>
    </button>
  )
}
