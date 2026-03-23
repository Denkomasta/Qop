import { useTranslation } from 'react-i18next'
import {
  getGetApiQuizzesQuizIdQuestionsQuestionIdQueryKey,
  usePatchApiQuizzesQuizIdQuestionsQuestionId,
} from '@/api/generated/endpoints/quizzes/quizzes'
import { cn } from '@/lib/utils'
import { toast } from 'sonner'
import type { AxiosError } from 'axios'
import type { AspNetProblemDetails } from '@/api/custom-axios'
import { Clock } from 'lucide-react'
import { useQueryClient } from '@tanstack/react-query'

interface QuestionTimeLimitEditorProps {
  quizId: string
  questionId: string
  currentTimeLimit: number
}

export function QuestionTimeLimitEditor({
  quizId,
  questionId,
  currentTimeLimit,
}: QuestionTimeLimitEditorProps) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()

  const updateMutation = usePatchApiQuizzesQuizIdQuestionsQuestionId({
    mutation: {
      // Setting the new data in the get EP too
      onSuccess: (updatedQuestionData) => {
        queryClient.setQueryData(
          getGetApiQuizzesQuizIdQuestionsQuestionIdQueryKey(quizId, questionId),
          updatedQuestionData,
        )
      },
    },
  })

  const presets = [10, 20, 30, 60, 120]

  const handleTimeChange = async (seconds: number) => {
    if (seconds === currentTimeLimit) return

    try {
      await updateMutation.mutateAsync({
        quizId,
        questionId,
        data: { timeLimit: seconds },
      })
    } catch (e) {
      const axiosError = e as AxiosError<AspNetProblemDetails>
      if (axiosError.response?.status === 403) {
        toast.error(t('errors.accessDeniedTitle'))
      } else {
        toast.error(t('common.error'))
      }
    }
  }

  return (
    <div className="space-y-3">
      <div className="flex items-center gap-2 text-muted-foreground">
        <Clock className="h-4 w-4" />
        <label className="text-xs font-black tracking-widest uppercase">
          {t('editor.timeLimit')}
        </label>
      </div>

      <div className="flex flex-wrap gap-2">
        {presets.map((time) => (
          <button
            key={time}
            onClick={() => handleTimeChange(time)}
            disabled={updateMutation.isPending}
            className={cn(
              'h-10 min-w-15 flex-1 rounded-lg border text-sm font-bold transition-all active:scale-95',
              currentTimeLimit === time
                ? 'border-primary bg-primary text-primary-foreground shadow-md'
                : 'border-muted bg-background text-muted-foreground hover:border-primary/50',
              updateMutation.isPending &&
                currentTimeLimit !== time &&
                'cursor-not-allowed opacity-50',
            )}
          >
            {time}s
          </button>
        ))}
      </div>
      <p className="px-1 text-[10px] text-muted-foreground italic">
        {t('editor.timeLimitHint')}
      </p>
    </div>
  )
}
