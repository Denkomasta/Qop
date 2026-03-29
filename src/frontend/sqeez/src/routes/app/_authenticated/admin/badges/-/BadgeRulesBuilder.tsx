import { useTranslation } from 'react-i18next'
import {
  Controller,
  useFieldArray,
  useFormContext,
  type FieldError,
} from 'react-hook-form'
import { Trash2, Plus } from 'lucide-react'

import { Input } from '@/components/ui/Input'
import { Button } from '@/components/ui/Button'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/Select'

import {
  METRIC_TRANSLATIONS,
  OPERATOR_MAP,
} from '@/constants/badgeRulesMappings'
import type { BadgeFormValues } from '@/schemas/badgeSchema'

export function BadgeRulesBuilder() {
  const { t } = useTranslation()

  const {
    control,
    register,
    formState: { errors },
  } = useFormContext<BadgeFormValues>()

  const { fields, append, remove } = useFieldArray({
    control,
    name: 'rules',
  })

  const rulesRootError = errors.rules?.root as FieldError | undefined

  return (
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
              id: null,
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

      {rulesRootError && (
        <p className="text-sm font-medium text-destructive">
          {rulesRootError.message}
        </p>
      )}

      <div className="flex flex-col gap-3">
        {fields.map((field, index) => (
          <div
            key={field.id}
            className="flex flex-col gap-4 rounded-lg border border-border bg-card p-4 shadow-sm md:flex-row md:items-end"
          >
            <div className="min-w-0 flex-1">
              <label className="mb-1 block text-xs font-medium text-muted-foreground">
                Metric
              </label>
              <Controller
                name={`rules.${index}.metric`}
                control={control}
                render={({ field }) => (
                  <Select onValueChange={field.onChange} value={field.value}>
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
                    <Select onValueChange={field.onChange} value={field.value}>
                      <SelectTrigger className="h-10 w-full bg-background">
                        <SelectValue placeholder="Operator" />
                      </SelectTrigger>
                      <SelectContent>
                        {Object.entries(OPERATOR_MAP).map(([key, symbol]) => (
                          <SelectItem key={key} value={key}>
                            <span className="mr-2 font-bold text-primary">
                              {symbol}
                            </span>
                            <span className="text-xs text-muted-foreground">
                              {key}
                            </span>
                          </SelectItem>
                        ))}
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
  )
}
