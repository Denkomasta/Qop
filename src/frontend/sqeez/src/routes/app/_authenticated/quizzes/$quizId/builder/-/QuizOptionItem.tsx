import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import {
  Trash2,
  CheckCircle2,
  Circle,
  Type,
  Image as ImageIcon,
} from 'lucide-react'
import {
  getGetApiQuizzesQuizIdQuestionsQuestionIdOptionsQueryKey,
  usePatchApiQuizzesQuizIdQuestionsQuestionIdOptionsOptionId,
  useDeleteApiQuizzesQuizIdQuestionsQuestionIdOptionsOptionId,
} from '@/api/generated/endpoints/quizzes/quizzes'
import { Button } from '@/components/ui/Button'
import { DebouncedInput } from '@/components/ui/Input/DebouncedInput'
import { useQueryClient } from '@tanstack/react-query'
import { cn } from '@/lib/utils'
import { toast } from 'sonner'
import { OptionMediaModal } from './OptionMediaModal'
import type { QuizOptionDto } from '@/api/generated/model'

interface QuizOptionItemProps {
  quizId: string
  questionId: string
  option: QuizOptionDto
}

export function QuizOptionItem({
  quizId,
  questionId,
  option,
}: QuizOptionItemProps) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const [isMediaModalOpen, setIsMediaModalOpen] = useState(false)

  const patchOption =
    usePatchApiQuizzesQuizIdQuestionsQuestionIdOptionsOptionId({
      mutation: {
        onSuccess: () => {
          queryClient.invalidateQueries({
            queryKey: getGetApiQuizzesQuizIdQuestionsQuestionIdOptionsQueryKey(
              quizId,
              questionId,
            ),
          })
        },
      },
    })

  const deleteOption =
    useDeleteApiQuizzesQuizIdQuestionsQuestionIdOptionsOptionId({
      mutation: {
        onSuccess: () => {
          queryClient.invalidateQueries({
            queryKey: getGetApiQuizzesQuizIdQuestionsQuestionIdOptionsQueryKey(
              quizId,
              questionId,
            ),
          })
        },
      },
    })

  const handleUpdate = async (data: any) => {
    try {
      await patchOption.mutateAsync({
        quizId,
        questionId,
        optionId: option.id.toString(),
        data,
      })
    } catch {
      toast.error(t('common.error'))
    }
  }

  const handleDelete = async () => {
    try {
      await deleteOption.mutateAsync({
        quizId,
        questionId,
        optionId: option.id.toString(),
      })
    } catch {
      toast.error(t('common.error'))
    }
  }

  return (
    <>
      <div
        className={cn(
          'group flex items-center gap-3 rounded-xl border p-2 transition-all',
          option.isCorrect
            ? 'border-green-500/30 bg-green-500/5'
            : 'border-border bg-background',
        )}
      >
        <button
          onClick={() => handleUpdate({ isCorrect: !option.isCorrect })}
          disabled={patchOption.isPending}
          className="shrink-0 p-2 transition-transform active:scale-90 disabled:opacity-50"
          title={t('editor.markCorrect')}
        >
          {option.isCorrect ? (
            <CheckCircle2 className="h-6 w-6 fill-green-500/20 text-green-500" />
          ) : (
            <Circle className="h-6 w-6 text-muted-foreground/30 hover:text-muted-foreground" />
          )}
        </button>

        <div className="flex flex-1 flex-col gap-1">
          <DebouncedInput
            value={option.text ?? ''}
            onChange={(newText) => handleUpdate({ text: newText })}
            placeholder={
              option.isFreeText
                ? t('editor.freeTextAnswerHint')
                : t('editor.optionTextHint')
            }
            className={cn(
              'border-none bg-transparent shadow-none focus-visible:ring-1',
              option.isFreeText && 'font-mono text-primary',
            )}
          />
        </div>

        <div className="flex items-center gap-1 border-l border-border/50 px-2">
          <Button
            variant="ghost"
            size="icon"
            onClick={() => handleUpdate({ isFreeText: !option.isFreeText })}
            className={cn(
              'h-8 w-8',
              option.isFreeText
                ? 'bg-primary/10 text-primary'
                : 'text-muted-foreground',
            )}
            title={t('editor.toggleFreeText')}
          >
            <Type className="h-4 w-4" />
          </Button>

          <Button
            variant="ghost"
            size="icon"
            onClick={() => setIsMediaModalOpen(true)}
            className={cn(
              'h-8 w-8',
              option.mediaAssetId
                ? 'bg-blue-500/10 text-blue-500'
                : 'text-muted-foreground',
            )}
            title={t('editor.manageOptionMedia')}
          >
            <ImageIcon className="h-4 w-4" />
          </Button>
        </div>

        <Button
          variant="ghost"
          size="icon"
          disabled={deleteOption.isPending}
          className="h-8 w-8 text-destructive opacity-0 transition-opacity group-hover:opacity-100 hover:bg-destructive/10 hover:text-destructive"
          onClick={handleDelete}
        >
          <Trash2 className="h-4 w-4" />
        </Button>
      </div>

      <OptionMediaModal
        isOpen={isMediaModalOpen}
        onClose={() => setIsMediaModalOpen(false)}
        currentMediaAssetId={option.mediaAssetId}
        onSave={async (newAssetId) => {
          await handleUpdate({ mediaAssetId: newAssetId })
        }}
      />
    </>
  )
}
