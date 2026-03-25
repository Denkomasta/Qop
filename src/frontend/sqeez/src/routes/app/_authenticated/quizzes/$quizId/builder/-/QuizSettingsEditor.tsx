import { useTranslation } from 'react-i18next'
import {
  getGetApiQuizzesQuizIdQueryKey,
  useGetApiQuizzesQuizId,
  usePatchApiQuizzesQuizId,
} from '@/api/generated/endpoints/quizzes/quizzes'
import { Loader2, Settings2, Globe, Globe2 } from 'lucide-react'
import { DebouncedInput } from '@/components/ui/Input/DebouncedInput'
import { Button } from '@/components/ui/Button'
import { DebouncedTextArea } from '@/components/ui/TextArea'
import { useQueryClient } from '@tanstack/react-query'
import { DateTimePicker, Input } from '@/components/ui/Input'

interface QuizSettingsEditorProps {
  quizId: string
}

export function QuizSettingsEditor({ quizId }: QuizSettingsEditorProps) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()

  const { data: quizResponse, isLoading } = useGetApiQuizzesQuizId(
    quizId,
    undefined,
    {
      query: { enabled: !!quizId },
    },
  )
  const patchMutation = usePatchApiQuizzesQuizId({
    mutation: {
      onSuccess: (updatedQuizData) => {
        queryClient.setQueryData(
          getGetApiQuizzesQuizIdQueryKey(quizId),
          updatedQuizData,
        )
      },
    },
  })

  const quiz = quizResponse

  const handleUpdate = async (field: string, value: string | number | null) => {
    await patchMutation.mutateAsync({
      quizId,
      data: { [field]: value },
    })
  }

  const togglePublish = async () => {
    const isPublished = !!quiz?.publishDate
    await handleUpdate(
      'publishDate',
      isPublished ? null : new Date().toISOString(),
    )
  }

  if (isLoading) {
    return (
      <div className="flex flex-1 items-center justify-center bg-background">
        <Loader2 className="h-8 w-8 animate-spin text-primary/30" />
      </div>
    )
  }

  const isPublished = !!quiz?.publishDate

  return (
    <div className="flex-1 overflow-y-auto bg-background p-8 lg:p-12">
      <div className="mx-auto max-w-3xl space-y-8">
        <div className="flex items-center justify-between border-b border-border pb-6">
          <div className="flex items-center gap-2">
            <Settings2 className="h-6 w-6 text-primary" />
            <h1 className="text-2xl font-bold tracking-tight">
              {t('editor.quizSettings')}
            </h1>
          </div>
          <Button
            variant={isPublished ? 'outline' : 'default'}
            onClick={togglePublish}
            disabled={patchMutation.isPending}
            className="gap-2 shadow-sm"
          >
            {isPublished ? (
              <>
                <Globe className="h-4 w-4 text-muted-foreground" />
                {t('editor.unpublish')}
              </>
            ) : (
              <>
                <Globe2 className="h-4 w-4" />
                {t('editor.publish')}
              </>
            )}
          </Button>
        </div>

        <DebouncedInput
          value={quiz?.title ?? ''}
          label={t('editor.quizTitle')}
          onChange={(val) => handleUpdate('title', val)}
          placeholder={t('editor.untitledQuiz')}
          className="text-lg font-semibold"
          debounceTime={800}
        />

        <DebouncedTextArea
          label={t('editor.quizDescription')}
          initialValue={quiz?.description ?? ''}
          onSave={async (val) => handleUpdate('description', val)}
          placeholder={t('editor.descriptionPlaceholder')}
          savingText={t('common.saving')}
          savedText={t('common.saved')}
          errorText={t('common.error')}
        />

        <div className="grid grid-cols-1 gap-8 rounded-xl border border-border bg-muted/5 p-6 md:grid-cols-2">
          <Input
            label={t('editor.maxRetries')}
            type="number"
            min="0"
            value={quiz?.maxRetries ?? 0}
            onChange={(e) =>
              handleUpdate('maxRetries', parseInt(e.target.value) || 0)
            }
            helpText={t('editor.maxRetriesHelp')}
          />

          <DateTimePicker
            label={t('editor.closingDate')}
            helpText={t('editor.closingDateHelp')}
            value={quiz?.closingDate}
            onChange={(isoString) => handleUpdate('closingDate', isoString)}
          />
        </div>
      </div>
    </div>
  )
}
