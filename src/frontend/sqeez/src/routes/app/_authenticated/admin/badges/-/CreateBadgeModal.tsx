import { useState, useRef } from 'react'
import { useTranslation } from 'react-i18next'
import { useForm, FormProvider } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Camera } from 'lucide-react'
import { toast } from 'sonner'
import { useQueryClient } from '@tanstack/react-query'

import { BaseModal } from '@/components/ui/Modal'
import { Input } from '@/components/ui/Input'
import { TextArea } from '@/components/ui/TextArea'
import { Button, AsyncButton } from '@/components/ui/Button'

import { usePostApiBadges } from '@/api/generated/endpoints/badges/badges'
import { getGetApiBadgesQueryKey } from '@/api/generated/endpoints/badges/badges'
import { BadgeRulesBuilder } from './BadgeRulesBuilder'
import { getBadgeSchema } from '@/schemas/badgeSchema'

interface CreateBadgeModalProps {
  isOpen: boolean
  onClose: () => void
}

export function CreateBadgeModal({ isOpen, onClose }: CreateBadgeModalProps) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const fileInputRef = useRef<HTMLInputElement>(null)
  const schema = getBadgeSchema(t)

  const [selectedFile, setSelectedFile] = useState<File | null>(null)
  const [previewUrl, setPreviewUrl] = useState<string | null>(null)

  type CreateBadgeFormValues = z.infer<typeof schema>

  const methods = useForm<CreateBadgeFormValues>({
    resolver: zodResolver(schema),
    mode: 'onChange',
    defaultValues: {
      name: '',
      description: '',
      xpBonus: 0,
      rules: [
        {
          metric: 'ScorePercentage',
          operator: 'GreaterThanOrEqual',
          targetValue: 1,
        },
      ],
    },
  })

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isValid },
  } = methods

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (file) {
      if (file.size > 2 * 1024 * 1024) {
        toast.error(t('errors.fileTooLarge', { maxValue: 2 }))
        return
      }
      setSelectedFile(file)
      setPreviewUrl(URL.createObjectURL(file))
    }
  }

  const handleModalClose = () => {
    reset()
    setSelectedFile(null)
    if (previewUrl) {
      URL.revokeObjectURL(previewUrl)
      setPreviewUrl(null)
    }
    onClose()
  }

  const createBadgeMutation = usePostApiBadges({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiBadgesQueryKey() })
        toast.success(t('admin.badges.createdSuccessfully'))
        handleModalClose()
      },
      onError: () => toast.error(t('common.error')),
    },
  })

  const onSubmit = async (data: CreateBadgeFormValues) => {
    if (!selectedFile) {
      toast.error(t('admin.badges.iconRequired'))
      return
    }

    try {
      await createBadgeMutation.mutateAsync({
        data: {
          Name: data.name,
          Description: data.description,
          XpBonus: data.xpBonus,
          IconFile: selectedFile,
          Rules: data.rules.map((rule) => ({
            metric: rule.metric,
            operator: rule.operator,
            targetValue: rule.targetValue,
          })),
        },
      })
    } catch (error) {
      console.error('Failed to create badge:', error)
    }
  }

  return (
    <BaseModal
      isOpen={isOpen}
      onClose={handleModalClose}
      title={t('admin.badges.createBadge')}
      description={t('admin.badges.createDesc')}
      className="sm:max-w-fit"
      footer={
        <div className="flex w-full justify-center gap-4 sm:space-x-0">
          <Button
            variant="outline"
            size="lg"
            onClick={handleModalClose}
            className="min-w-32"
          >
            {t('common.cancel')}
          </Button>
          <AsyncButton
            size="lg"
            onClick={handleSubmit(onSubmit)}
            disabled={!isValid || !selectedFile}
            isLoading={createBadgeMutation.isPending}
            loadingText={t('common.saving') + '...'}
            className="min-w-32"
          >
            {t('common.create')}
          </AsyncButton>
        </div>
      }
    >
      <FormProvider {...methods}>
        <div className="mr-4 flex max-h-[55vh] w-full max-w-90 flex-col gap-8 overflow-y-auto">
          <div className="flex flex-col gap-6">
            <div className="flex flex-col items-center gap-2 pt-2">
              <input
                type="file"
                accept="image/jpeg, image/png, image/svg+xml"
                className="hidden"
                ref={fileInputRef}
                onChange={handleFileChange}
              />
              {previewUrl ? (
                <div className="relative">
                  <div className="flex size-32 items-center justify-center rounded-xl border border-border bg-muted/30 p-2 shadow-sm">
                    <img
                      src={previewUrl}
                      alt="Preview"
                      className="h-full w-full object-contain"
                    />
                  </div>
                  <Button
                    size="sm"
                    variant="secondary"
                    className="absolute -right-3 -bottom-3 rounded-full shadow-md"
                    onClick={() => fileInputRef.current?.click()}
                  >
                    <Camera className="size-4" />
                  </Button>
                </div>
              ) : (
                <div
                  className="flex size-32 cursor-pointer flex-col items-center justify-center rounded-xl border-2 border-dashed border-muted-foreground/50 bg-secondary/30 transition-colors hover:bg-secondary/50"
                  onClick={() => fileInputRef.current?.click()}
                >
                  <Camera className="mb-2 size-8 text-muted-foreground" />
                  <span className="text-center text-xs font-medium text-muted-foreground">
                    {t('admin.badges.uploadIcon')}
                  </span>
                </div>
              )}
              {!selectedFile && (
                <p className="text-xs text-destructive">
                  {t('common.required')}
                </p>
              )}
            </div>

            <div className="flex flex-col gap-4">
              <Input
                label={t('common.name')}
                placeholder={t('admin.badges.namePlaceholder')}
                error={errors.name?.message}
                {...register('name')}
              />
              <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                <Input
                  type="number"
                  label={t('admin.badges.xpBonus')}
                  placeholder="100"
                  min={0}
                  error={errors.xpBonus?.message}
                  {...register('xpBonus', { valueAsNumber: true })}
                />
              </div>
              <TextArea
                label={t('common.description')}
                placeholder={t('admin.badges.descPlaceholder')}
                error={errors.description?.message}
                {...register('description')}
              />
            </div>
          </div>

          <hr className="border-border" />

          <BadgeRulesBuilder />
        </div>
      </FormProvider>
    </BaseModal>
  )
}
