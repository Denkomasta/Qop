import { z } from 'zod'
import type { TFunction } from 'i18next'
import {
  METRIC_TRANSLATIONS,
  OPERATOR_MAP,
} from '@/constants/badgeRulesMappings'
import type { BadgeMetric, BadgeOperator } from '@/api/generated/model'

const metricKeys = Object.keys(METRIC_TRANSLATIONS) as [
  BadgeMetric,
  ...BadgeMetric[],
]
const operatorKeys = Object.keys(OPERATOR_MAP) as [
  BadgeOperator,
  ...BadgeOperator[],
]

export const getBadgeSchema = (t: TFunction) =>
  z.object({
    name: z.string().min(1, t('common.required')),
    description: z.string().min(1, t('common.required')),
    xpBonus: z.number().min(0, t('errors.invalidNumber')),
    rules: z
      .array(
        z.object({
          id: z.union([z.number(), z.string()]).nullable().optional(),
          metric: z.enum(metricKeys),
          operator: z.enum(operatorKeys),
          targetValue: z.number().min(0, t('common.required')),
        }),
      )
      .min(1, t('admin.badges.atLeastOneRule')),
  })

export type BadgeFormValues = z.infer<ReturnType<typeof getBadgeSchema>>
