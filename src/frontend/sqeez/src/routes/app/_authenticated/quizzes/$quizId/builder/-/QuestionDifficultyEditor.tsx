import { useTranslation } from 'react-i18next'
import {
  usePatchApiQuizzesQuizIdQuestionsQuestionId,
  getGetApiQuizzesQuizIdQuestionsQuestionIdQueryKey,
} from '@/api/generated/endpoints/quizzes/quizzes'
import { useQueryClient } from '@tanstack/react-query'
import { cn } from '@/lib/utils'
import { toast } from 'sonner'
import type { AxiosError } from 'axios'
import type { AspNetProblemDetails } from '@/api/custom-axios'

interface QuestionDifficultyEditorProps {
  quizId: string
  questionId: string
  currentDifficulty: number
}

export function QuestionDifficultyEditor({
  quizId,
  questionId,
  currentDifficulty,
}: QuestionDifficultyEditorProps) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()

  const updateMutation = usePatchApiQuizzesQuizIdQuestionsQuestionId({
    mutation: {
      onSuccess: (updatedQuestionData) => {
        queryClient.setQueryData(
          getGetApiQuizzesQuizIdQuestionsQuestionIdQueryKey(quizId, questionId),
          updatedQuestionData,
        )
      },
    },
  })

  const levels = [1, 2, 3, 4, 5]

  const handleDifficultyChange = async (level: number) => {
    if (level === currentDifficulty) return

    try {
      await updateMutation.mutateAsync({
        quizId,
        questionId,
        data: { difficulty: level },
      })
    } catch (e) {
      const axiosError = e as AxiosError<AspNetProblemDetails>
      if (axiosError.response?.status === 403) {
        toast.error(t('errors.accessDeniedTitle'))
      } else {
        toast.error(t('common.error') || 'Failed to save')
      }
    }
  }

  return (
    <div className="flex flex-col gap-2">
      <div className="text-xs font-black tracking-widest text-muted-foreground uppercase">
        {t('editor.difficulty')}
      </div>

      <div className="flex gap-2">
        {levels.map((level) => (
          <button
            key={level}
            onClick={() => handleDifficultyChange(level)}
            disabled={updateMutation.isPending}
            className={cn(
              'h-10 flex-1 rounded-lg border text-sm font-bold transition-all active:scale-95',
              currentDifficulty === level
                ? 'border-primary bg-primary text-primary-foreground shadow-md'
                : 'border-muted bg-background text-muted-foreground hover:border-primary/50',
            )}
          >
            {level}
          </button>
        ))}
      </div>

      <p className="px-1 text-[10px] text-muted-foreground italic">
        {currentDifficulty <= 2 && 'Easy - Basic concepts'}
        {currentDifficulty === 3 && 'Medium - Requires some thought'}
        {currentDifficulty >= 4 && 'Hard - Advanced mastery'}
      </p>
    </div>
  )
}
