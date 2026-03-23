import { useTranslation } from 'react-i18next'
import { Plus, Loader2 } from 'lucide-react'
import {
  getGetApiQuizzesQuizIdQuestionsQuestionIdOptionsQueryKey,
  useGetApiQuizzesQuizIdQuestionsQuestionIdOptions,
  usePostApiQuizzesQuizIdQuestionsQuestionIdOptions,
} from '@/api/generated/endpoints/quizzes/quizzes'
import { Button } from '@/components/ui/Button'
import { useQueryClient } from '@tanstack/react-query'
import { QuizOptionItem } from './QuizOptionItem'

interface QuizOptionsEditorProps {
  quizId: string
  questionId: string
}

export function QuizOptionsEditor({
  quizId,
  questionId,
}: QuizOptionsEditorProps) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()

  const { data: optionsData, isLoading } =
    useGetApiQuizzesQuizIdQuestionsQuestionIdOptions(quizId, questionId)

  const options = optionsData?.data ?? []

  const addOptionMutation = usePostApiQuizzesQuizIdQuestionsQuestionIdOptions({
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

  const handleAddOption = async () => {
    await addOptionMutation.mutateAsync({
      quizId,
      questionId,
      data: {
        text: t('editor.newOptionDefault'),
        isCorrect: false,
        isFreeText: false,
      },
    })
  }

  if (isLoading)
    return (
      <Loader2 className="mx-auto mt-4 h-6 w-6 animate-spin text-primary" />
    )

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <label className="text-xs font-black tracking-widest text-muted-foreground uppercase">
          {t('editor.options')}
        </label>
        <Button
          variant="ghost"
          size="sm"
          onClick={handleAddOption}
          disabled={addOptionMutation.isPending}
          className="h-8 gap-1"
        >
          {addOptionMutation.isPending ? (
            <Loader2 className="h-3.5 w-3.5 animate-spin" />
          ) : (
            <Plus className="h-3.5 w-3.5" />
          )}
          {t('editor.addOption')}
        </Button>
      </div>

      <div className="space-y-3">
        {options.map((opt) => (
          <QuizOptionItem
            key={opt.id}
            quizId={quizId}
            questionId={questionId}
            option={opt}
          />
        ))}

        {options.length === 0 && (
          <div className="rounded-xl border-2 border-dashed border-muted/50 p-8 text-center">
            <p className="text-sm text-muted-foreground">
              {t('editor.noOptionsYet')}
            </p>
          </div>
        )}
      </div>
    </div>
  )
}
