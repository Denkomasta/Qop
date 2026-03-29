import { useState, useRef } from 'react'
import { useTranslation } from 'react-i18next'
import { useForm, Controller, useFieldArray } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Plus, Trash2, Camera } from 'lucide-react'
import { toast } from 'sonner'
import { useQueryClient } from '@tanstack/react-query'

import { BaseModal } from '@/components/ui/Modal'
import { Input } from '@/components/ui/Input'
import { TextArea } from '@/components/ui/TextArea'
import { Button, AsyncButton } from '@/components/ui/Button'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/Select'

import { usePostApiBadges } from '@/api/generated/endpoints/badges/badges'
import { getGetApiBadgesQueryKey } from '@/api/generated/endpoints/badges/badges'
import {
  METRIC_TRANSLATIONS,
  OPERATOR_MAP,
} from '@/constants/badgeRulesMappings'
import type { BadgeMetric, BadgeOperator } from '@/api/generated/model'

interface CreateBadgeModalProps {
  isOpen: boolean
  onClose: () => void
}

export function CreateBadgeModal({ isOpen, onClose }: CreateBadgeModalProps) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const fileInputRef = useRef<HTMLInputElement>(null)

  const [selectedFile, setSelectedFile] = useState<File | null>(null)
  const [previewUrl, setPreviewUrl] = useState<string | null>(null)

  const metricKeys = Object.keys(METRIC_TRANSLATIONS) as [
    BadgeMetric,
    ...BadgeMetric[],
  ]
  const operatorKeys = Object.keys(OPERATOR_MAP) as [
    BadgeOperator,
    ...BadgeOperator[],
  ]

  const createBadgeSchema = z.object({
    name: z.string().min(1, t('common.required')),
    description: z.string().min(1, t('common.required')),
    xpBonus: z.number().min(0, t('errors.invalidNumber')),
    rules: z
      .array(
        z.object({
          metric: z.enum(metricKeys),
          operator: z.enum(operatorKeys),
          targetValue: z.number().min(0, t('common.required')),
        }),
      )
      .min(1, t('admin.badges.atLeastOneRule')),
  })

  type CreateBadgeFormValues = z.infer<typeof createBadgeSchema>

  const {
    register,
    handleSubmit,
    control,
    reset,
    formState: { errors, isValid },
  } = useForm<CreateBadgeFormValues>({
    resolver: zodResolver(createBadgeSchema),
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

  const { fields, append, remove } = useFieldArray({
    control,
    name: 'rules',
  })

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
              <p className="text-xs text-destructive">{t('common.required')}</p>
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

        <div className="flex flex-col gap-4">
          <div className="flex flex-col items-start gap-3 sm:flex-row sm:items-center sm:justify-between">
            <div className="min-w-0">
              <h3 className="text-lg font-semibold text-foreground">
                {t('admin.badges.rulesTitle')}
              </h3>
              <p className="text-sm text-muted-foreground">
                {t('admin.badges.rulesDesc')}
              </p>
            </div>
            <Button
              type="button"
              variant="outline"
              size="sm"
              className="shrink-0"
              onClick={() =>
                append({
                  metric: 'ScorePercentage',
                  operator: 'GreaterThanOrEqual',
                  targetValue: 80,
                })
              }
            >
              <Plus className="mr-2 h-4 w-4" />
              {t('admin.badges.addRule')}
            </Button>
          </div>

          {errors.rules?.root && (
            <p className="text-sm font-medium text-destructive">
              {errors.rules.root.message}
            </p>
          )}

          <div className="flex flex-col gap-3">
            {fields.map((field, index) => (
              <div
                key={field.id}
                className="flex flex-col gap-4 rounded-lg border border-border bg-card p-4 shadow-sm"
              >
                <div className="min-w-0 flex-1">
                  <label className="mb-1 block text-xs font-medium text-muted-foreground">
                    Metric
                  </label>
                  <Controller
                    name={`rules.${index}.metric`}
                    control={control}
                    render={({ field }) => (
                      <Select
                        onValueChange={field.onChange}
                        value={field.value}
                      >
                        <SelectTrigger className="h-10 w-full bg-background">
                          <SelectValue placeholder="Select a metric" />
                        </SelectTrigger>
                        <SelectContent>
                          {Object.entries(METRIC_TRANSLATIONS).map(
                            ([key, value]) => (
                              <SelectItem key={key} value={key}>
                                {t(value, key)}
                              </SelectItem>
                            ),
                          )}
                        </SelectContent>
                      </Select>
                    )}
                  />
                </div>

                <div className="flex w-full items-end gap-3 md:w-auto">
                  <div className="min-w-0 flex-1 md:w-36 md:flex-none">
                    <label className="mb-1 block text-xs font-medium text-muted-foreground">
                      Operator
                    </label>
                    <Controller
                      name={`rules.${index}.operator`}
                      control={control}
                      render={({ field }) => (
                        <Select
                          onValueChange={field.onChange}
                          value={field.value}
                        >
                          <SelectTrigger className="h-10 w-full bg-background">
                            <SelectValue placeholder="Operator" />
                          </SelectTrigger>
                          <SelectContent>
                            {Object.entries(OPERATOR_MAP).map(
                              ([key, symbol]) => (
                                <SelectItem key={key} value={key}>
                                  <span className="mr-2 font-bold text-primary">
                                    {symbol}
                                  </span>
                                  <span className="text-xs text-muted-foreground">
                                    {key}
                                  </span>
                                </SelectItem>
                              ),
                            )}
                          </SelectContent>
                        </Select>
                      )}
                    />
                  </div>

                  <div className="w-20 shrink-0 sm:w-24">
                    <label className="mb-1 block text-xs font-medium text-muted-foreground">
                      Value
                    </label>
                    <Input
                      type="number"
                      min={0}
                      {...register(`rules.${index}.targetValue`, {
                        valueAsNumber: true,
                      })}
                      className="h-10"
                      hideErrors
                    />
                  </div>

                  <div className="flex shrink-0 pb-0.5">
                    <Button
                      type="button"
                      variant="ghost"
                      size="icon"
                      className="h-9 w-9 text-muted-foreground hover:bg-destructive/10 hover:text-destructive"
                      onClick={() => remove(index)}
                      disabled={fields.length === 1}
                    >
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </BaseModal>
  )
}
