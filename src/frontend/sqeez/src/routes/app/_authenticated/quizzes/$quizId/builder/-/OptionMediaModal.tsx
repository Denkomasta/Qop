import { useRef, useState, useEffect } from 'react'
import { useTranslation } from 'react-i18next'
import { UploadCloud, X, FileAudio, FileVideo, ImageIcon } from 'lucide-react'
import { BaseModal } from '@/components/ui/Modal'
import { AsyncButton, Button } from '@/components/ui/Button'
import { toast } from 'sonner'
import { usePostApiMediaAssetsUpload } from '@/api/generated/endpoints/media-assets/media-assets'
import { MediaAssetViewer } from '../../play/-/MediaAssetViewer'

interface OptionMediaModalProps {
  isOpen: boolean
  onClose: () => void
  onSave: (newMediaAssetId: number | string | null) => Promise<void>
  currentMediaAssetId?: number | string | null
  maxFileSizeMB?: number
}

export function OptionMediaModal({
  isOpen,
  onClose,
  onSave,
  currentMediaAssetId,
  maxFileSizeMB = 10,
}: OptionMediaModalProps) {
  const { t } = useTranslation()
  const fileInputRef = useRef<HTMLInputElement>(null)

  const [selectedFile, setSelectedFile] = useState<File | null>(null)
  const [previewUrl, setPreviewUrl] = useState<string | null>(null)
  const [isConfirmingRemove, setIsConfirmingRemove] = useState(false)

  const uploadMedia = usePostApiMediaAssetsUpload()

  useEffect(() => {
    return () => {
      if (previewUrl) URL.revokeObjectURL(previewUrl)
    }
  }, [previewUrl])

  const handleClose = () => {
    setSelectedFile(null)
    setIsConfirmingRemove(false)
    if (previewUrl) {
      URL.revokeObjectURL(previewUrl)
      setPreviewUrl(null)
    }
    onClose()
  }

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (file) {
      if (file.size > maxFileSizeMB * 1024 * 1024) {
        toast.error(t('errors.fileTooLarge', { maxValue: maxFileSizeMB }))
        return
      }
      setSelectedFile(file)
      setPreviewUrl(URL.createObjectURL(file))
    }
  }

  const handleConfirmSave = async () => {
    try {
      if (selectedFile) {
        const response = await uploadMedia.mutateAsync({
          data: { File: selectedFile },
        })

        if (!response.id) throw new Error('Upload failed to return an ID')

        await onSave(response.id)
        toast.success(t('editor.mediaUploaded'))
      } else {
        handleClose()
        return
      }
      handleClose()
    } catch (error) {
      console.error(error)
      toast.error(t('common.error'))
    }
  }

  const handleRemoveCurrentMedia = async () => {
    try {
      await onSave(0) // never existing id
      toast.success(t('editor.mediaRemoved'))
      handleClose()
    } catch {
      toast.error(t('common.error'))
    }
  }

  const renderLocalPreview = () => {
    if (!selectedFile || !previewUrl) return null

    if (selectedFile.type.startsWith('image/')) {
      return (
        <img
          src={previewUrl}
          alt="Local Preview"
          className="max-h-48 w-full rounded-xl object-contain"
        />
      )
    }
    if (selectedFile.type.startsWith('video/')) {
      return (
        <div className="flex flex-col items-center justify-center rounded-xl bg-muted/20 p-4">
          <FileVideo className="mb-2 h-12 w-12 text-muted-foreground" />
          <p className="text-sm font-medium">{selectedFile.name}</p>
        </div>
      )
    }
    if (selectedFile.type.startsWith('audio/')) {
      return (
        <div className="flex flex-col items-center justify-center rounded-xl bg-muted/20 p-4">
          <FileAudio className="mb-2 h-12 w-12 text-muted-foreground" />
          <p className="text-sm font-medium">{selectedFile.name}</p>
        </div>
      )
    }
    return <p className="text-sm">{selectedFile.name}</p>
  }

  return (
    <BaseModal
      isOpen={isOpen}
      onClose={handleClose}
      title={t('editor.optionMediaTitle')}
      description={t('editor.optionMediaDescription')}
      footer={
        isConfirmingRemove ? (
          <div className="flex w-full animate-in flex-col items-center justify-evenly gap-4 fade-in slide-in-from-bottom-2 sm:flex-row">
            <Button
              variant="outline"
              size="lg"
              onClick={() => setIsConfirmingRemove(false)}
              className="flex-1 sm:min-w-32 sm:flex-none"
            >
              {t('common.cancel')}
            </Button>
            <AsyncButton
              variant="destructive"
              size="lg"
              onClick={handleRemoveCurrentMedia}
              className="flex-1 sm:min-w-32 sm:flex-none"
            >
              {t('common.remove')}
            </AsyncButton>
          </div>
        ) : (
          <div className="flex w-full animate-in justify-between gap-4 fade-in sm:space-x-0">
            {currentMediaAssetId && !selectedFile ? (
              <Button
                variant="destructive"
                size="lg"
                onClick={() => setIsConfirmingRemove(true)}
                className="min-w-32"
              >
                {t('common.remove')}
              </Button>
            ) : (
              <div />
            )}

            <div className="flex gap-2">
              <Button
                variant="outline"
                size="lg"
                onClick={handleClose}
                className="min-w-32"
              >
                {t('common.cancel')}
              </Button>
              <AsyncButton
                size="lg"
                onClick={handleConfirmSave}
                disabled={!selectedFile}
                loadingText={t('common.saving') + '...'}
                className="min-w-32"
              >
                {t('common.save')}
              </AsyncButton>
            </div>
          </div>
        )
      }
    >
      <div className="flex w-full flex-col items-center gap-6 py-4">
        <input
          type="file"
          accept="image/*, video/*, audio/*"
          className="hidden"
          ref={fileInputRef}
          onChange={handleFileChange}
        />

        <div className="w-full">
          {selectedFile ? (
            <div className="group relative w-full rounded-xl border-2 border-primary/20 bg-primary/5 p-2">
              {renderLocalPreview()}
              <Button
                size="icon"
                variant="secondary"
                className="absolute top-2 right-2 rounded-full opacity-0 shadow-md transition-opacity group-hover:opacity-100"
                onClick={() => {
                  setSelectedFile(null)
                  if (previewUrl) URL.revokeObjectURL(previewUrl)
                }}
              >
                <X className="h-4 w-4" />
              </Button>
            </div>
          ) : currentMediaAssetId ? (
            <div className="group relative w-full">
              <MediaAssetViewer assetId={currentMediaAssetId} isOption />
              <Button
                size="sm"
                variant="secondary"
                className="absolute right-2 bottom-2 gap-2 shadow-md"
                onClick={() => fileInputRef.current?.click()}
              >
                <UploadCloud className="h-4 w-4" />
                {t('editor.changeMedia')}
              </Button>
            </div>
          ) : (
            <div
              className="flex h-40 w-full cursor-pointer flex-col items-center justify-center rounded-xl border-2 border-dashed border-muted-foreground/50 bg-secondary/30 transition-colors hover:bg-secondary/50"
              onClick={() => fileInputRef.current?.click()}
            >
              <ImageIcon className="mb-2 h-8 w-8 text-muted-foreground" />
              <span className="text-sm font-medium text-muted-foreground">
                {t('editor.clickToSelectMedia')}
              </span>
              <span className="mt-1 text-xs text-muted-foreground/70">
                Max {maxFileSizeMB}MB (Images, Video, Audio)
              </span>
            </div>
          )}
        </div>
      </div>
    </BaseModal>
  )
}
