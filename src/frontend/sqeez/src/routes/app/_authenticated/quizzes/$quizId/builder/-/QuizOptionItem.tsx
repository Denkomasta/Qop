import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Trash2, CheckCircle2, Circle, Image as ImageIcon } from 'lucide-react'
import {
  getGetApiQuizzesQuizIdQuestionsQuestionIdOptionsQueryKey,
  usePatchApiQuizzesQuizIdQuestionsQuestionIdOptionsOptionId,
  useDeleteApiQuizzesQuizIdQuestionsQuestionIdOptionsOptionId,
} from '@/api/generated/endpoints/quizzes/quizzes'
import { Button } from '@/components/ui/Button'
import { DebouncedInput } from '@/components/ui/Input/DebouncedInput'
import { useQueryClient } from '@tanstack/react-query'
import { cn } from '@/lib/utils'
import { OptionMediaModal } from './OptionMediaModal'
import type { PatchQuizOptionDto, QuizOptionDto } from '@/api/generated/model'
import { handleQuizMutationError } from '@/lib/quizHelpers'

interface QuizOptionItemProps {
  quizId: string
  questionId: string
  option: QuizOptionDto
  isFreeTextMode?: boolean
}

export function QuizOptionItem({
  quizId,
  questionId,
  option,
  isFreeTextMode,
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
        onError: (error) => handleQuizMutationError(error, t),
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
        onError: (error) => handleQuizMutationError(error, t),
      },
    })

  const handleUpdate = async (data: PatchQuizOptionDto) => {
    await patchOption.mutateAsync({
      quizId,
      questionId,
      optionId: option.id.toString(),
      data,
    })
  }

  const handleDelete = async () => {
    await deleteOption.mutateAsync({
      quizId,
      questionId,
      optionId: option.id.toString(),
    })
  }

  return (
    <>
      <div
        className={cn(
          'group flex items-center gap-3 rounded-xl border p-2 transition-all',
          option.isCorrect && !isFreeTextMode
            ? 'border-green-500/30 bg-green-500/5'
            : 'border-border bg-background',
          isFreeTextMode && 'border-blue-500/20 bg-blue-500/5',
        )}
      >
        {!isFreeTextMode && (
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
        )}

        <div className="flex flex-1 flex-col gap-1">
          <DebouncedInput
            value={option.text ?? ''}
            onChange={(newText) => handleUpdate({ text: newText })}
            placeholder={
              isFreeTextMode
                ? t('editor.freeTextAnswerHint')
                : t('editor.optionTextHint')
            }
            className={cn(
              'border-none bg-transparent shadow-none focus-visible:ring-1',
              isFreeTextMode && 'font-mono text-primary',
            )}
            hideErrors
          />
        </div>

        {!isFreeTextMode && (
          <div className="flex items-center gap-1 border-l border-border/50 px-2">
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
        )}

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

      {!isFreeTextMode && (
        <OptionMediaModal
          isOpen={isMediaModalOpen}
          onClose={() => setIsMediaModalOpen(false)}
          currentMediaAssetId={option.mediaAssetId}
          onSave={async (newAssetId) => {
            await handleUpdate({ mediaAssetId: newAssetId })
          }}
        />
      )}
    </>
  )
}
