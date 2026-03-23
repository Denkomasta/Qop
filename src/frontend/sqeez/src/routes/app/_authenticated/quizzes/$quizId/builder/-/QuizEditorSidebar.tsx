import { useTranslation } from 'react-i18next'
import { Plus, Loader2, List, Trash2 } from 'lucide-react'
import { useQuizEditorUIStore } from '@/store/useQuizEditorUIStore'
import {
  useDeleteApiQuizzesQuizIdQuestionsQuestionId,
  useGetApiQuizzesQuizIdQuestions,
  usePostApiQuizzesQuizIdQuestions,
} from '@/api/generated/endpoints/quizzes/quizzes'
import { Button } from '@/components/ui/Button'
import { cn } from '@/lib/utils'

interface QuizEditorSidebarProps {
  quizId: string
}

export function QuizEditorSidebar({ quizId }: QuizEditorSidebarProps) {
  const { t } = useTranslation()
  const { activeQuestionId, actions } = useQuizEditorUIStore()

  const {
    data: pagedResponse,
    isLoading,
    refetch,
  } = useGetApiQuizzesQuizIdQuestions(quizId)

  const questions = pagedResponse?.data ?? []

  const createQuestion = usePostApiQuizzesQuizIdQuestions()

  const handleAddQuestion = async () => {
    try {
      const response = await createQuestion.mutateAsync({
        quizId,
        data: {
          title: t('editor.newQuestionDefault'),
          quizId,
          difficulty: 1,
          mediaAssetId: null,
          timeLimit: 30,
        },
      })

      await refetch()
      if (response.id) {
        actions.selectQuestion(Number(response.id))
      }
    } catch (error) {
      console.error('Failed to create question', error)
    }
  }

  const deleteQuestion = useDeleteApiQuizzesQuizIdQuestionsQuestionId()

  const handleDeleteQuestion = async (
    e: React.MouseEvent,
    qId: number | string,
  ) => {
    e.stopPropagation()

    const confirmed = window.confirm(t('common.confirmDelete'))
    if (!confirmed) return

    try {
      await deleteQuestion.mutateAsync({
        quizId,
        questionId: qId.toString(),
      })

      if (activeQuestionId === qId) {
        actions.selectQuestion(null)
      }

      await refetch()
    } catch (error) {
      console.error('Failed to delete question', error)
    }
  }

  return (
    <aside className="flex h-full w-80 flex-col overflow-hidden border-r bg-muted/5">
      <div className="flex items-center justify-between border-b bg-background p-4 shadow-sm">
        <div className="flex items-center gap-2">
          <List className="h-4 w-4 text-primary" />
          <h2 className="text-xs font-bold tracking-widest text-foreground uppercase">
            {t('editor.questionsTitle')}
          </h2>
        </div>
        <Button
          variant="outline"
          size="sm"
          className="h-8 w-8 p-0"
          onClick={handleAddQuestion}
          disabled={createQuestion.isPending}
          title={t('editor.addQuestion')}
        >
          {createQuestion.isPending ? (
            <Loader2 className="h-4 w-4 animate-spin" />
          ) : (
            <Plus className="h-4 w-4" />
          )}
        </Button>
      </div>

      <div className="flex-1 space-y-2 overflow-y-auto p-3">
        {isLoading ? (
          <div className="flex flex-col items-center gap-2 py-10 opacity-50">
            <Loader2 className="h-5 w-5 animate-spin text-primary" />
            <span>{t('common.loading')}</span>
          </div>
        ) : questions.length === 0 ? (
          <div className="rounded-xl border-2 border-dashed border-muted/50 px-6 py-12 text-center">
            <p className="text-xs font-medium text-muted-foreground">
              {t('editor.noQuestionsYet')}
            </p>
          </div>
        ) : (
          questions.map((q, index) => (
            <div key={q.id} className="group relative w-full">
              <button
                onClick={() => actions.selectQuestion(Number(q.id))}
                className={cn(
                  'flex w-full items-center gap-3 rounded-xl border p-3 text-left transition-all',
                  activeQuestionId === q.id
                    ? 'border-primary bg-primary text-primary-foreground shadow-lg'
                    : 'border-transparent bg-background text-foreground hover:border-muted-foreground/20 hover:bg-muted/50',
                )}
              >
                <div
                  className={cn(
                    'flex h-7 w-7 shrink-0 items-center justify-center rounded-lg border text-xs font-black',
                    activeQuestionId === q.id
                      ? 'border-primary-foreground/40 bg-primary-foreground/20'
                      : 'bg-muted text-muted-foreground',
                  )}
                >
                  {index + 1}
                </div>

                <div className="flex flex-1 flex-col truncate pr-6">
                  <span className="truncate text-sm font-semibold">
                    {q.title || t('editor.untitledQuestion')}
                  </span>
                </div>
              </button>

              <Button
                variant="ghost"
                size="icon"
                className={cn(
                  'absolute top-1/2 right-2 h-7 w-7 -translate-y-1/2 opacity-0 transition-opacity group-hover:opacity-100',
                  activeQuestionId === q.id
                    ? 'text-primary-foreground hover:bg-primary-foreground/20 hover:text-primary-foreground'
                    : 'text-muted-foreground hover:bg-destructive/10 hover:text-destructive',
                )}
                onClick={(e) => handleDeleteQuestion(e, q.id)}
              >
                {deleteQuestion.isPending ? (
                  <Loader2 className="h-3 w-3 animate-spin" />
                ) : (
                  <Trash2 className="h-3 w-3" />
                )}
              </Button>
            </div>
          ))
        )}
      </div>
    </aside>
  )
}
