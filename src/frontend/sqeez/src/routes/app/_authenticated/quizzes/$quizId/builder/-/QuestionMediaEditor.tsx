import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { usePatchApiQuizzesQuizIdQuestionsQuestionId } from '@/api/generated/endpoints/quizzes/quizzes'
import { usePostApiMediaAssetsUpload } from '@/api/generated/endpoints/media-assets/media-assets'
import { Image as ImageIcon, UploadCloud, X, Loader2 } from 'lucide-react'
import { toast } from 'sonner'
import { MediaAssetViewer } from '../../play/-/MediaAssetViewer'
import { AsyncButton } from '@/components/ui'

interface QuestionMediaEditorProps {
  quizId: string
  questionId: string
  currentMediaAssetId: number | string | null
}

export function QuestionMediaEditor({
  quizId,
  questionId,
  currentMediaAssetId,
}: QuestionMediaEditorProps) {
  const { t } = useTranslation()
  const [isUploading, setIsUploading] = useState(false)

  const updateMutation = usePatchApiQuizzesQuizIdQuestionsQuestionId()

  const uploadMutation = usePostApiMediaAssetsUpload()

  const handleFileUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (!file) return

    setIsUploading(true)
    try {
      const uploadResponse = await uploadMutation.mutateAsync({
        data: {
          File: file,
          IsPrivate: false, // For now everything is public to use
        },
      })

      const newAssetId = uploadResponse.id

      if (!newAssetId) throw new Error('No asset ID returned from upload')

      await updateMutation.mutateAsync({
        quizId,
        questionId,
        data: { mediaAssetId: newAssetId },
      })

      toast.success(t('editor.mediaUploaded'))
    } catch (error) {
      console.error('Upload failed', error)
      toast.error(t('common.error'))
    } finally {
      setIsUploading(false)
      e.target.value = ''
    }
  }

  const handleRemoveMedia = async () => {
    try {
      await updateMutation.mutateAsync({
        quizId,
        questionId,
        data: { mediaAssetId: null },
      })
    } catch {
      toast.error(t('common.error'))
    }
  }

  return (
    <div className="space-y-3">
      <div className="flex items-center gap-2 text-xs font-black tracking-widest text-muted-foreground uppercase">
        <ImageIcon className="h-4 w-4" /> {t('editor.mediaAsset')}
      </div>

      {currentMediaAssetId ? (
        <div className="group relative w-full overflow-hidden rounded-xl border-2 border-muted bg-muted/10">
          <MediaAssetViewer assetId={currentMediaAssetId} />

          <AsyncButton
            onClick={handleRemoveMedia}
            disabled={updateMutation.isPending}
            className="hover:text-destructive-foreground rounded-md bg-background/80 p-2 text-destructive opacity-0 backdrop-blur transition-all group-hover:opacity-100 hover:bg-destructive"
            title={t('editor.removeMedia')}
          >
            <X className="size-4" />
          </AsyncButton>
        </div>
      ) : (
        <label className="flex h-32 w-full cursor-pointer flex-col items-center justify-center rounded-xl border-2 border-dashed border-muted-foreground/30 bg-muted/5 transition-colors hover:border-primary/50 hover:bg-primary/5">
          {isUploading ? (
            <div className="flex flex-col items-center gap-2">
              <Loader2 className="h-6 w-6 animate-spin text-primary" />
              <span className="text-xs font-medium text-muted-foreground">
                {t('common.loading')}
              </span>
            </div>
          ) : (
            <>
              <UploadCloud className="mb-2 h-6 w-6 text-muted-foreground" />
              <span className="text-sm font-medium text-muted-foreground">
                {t('editor.uploadMediaHint')}
              </span>
              <input
                type="file"
                accept="image/*,video/*,audio/*"
                className="hidden"
                onChange={handleFileUpload}
                disabled={isUploading}
              />
            </>
          )}
        </label>
      )}
    </div>
  )
}
