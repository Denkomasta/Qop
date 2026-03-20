import type { BadgeOperator, BadgeMetric } from '@/api/generated/model'
import type { TranslationKey } from '@/i18next'

export const OPERATOR_MAP: Record<BadgeOperator, string> = {
  Equals: '=',
  GreaterThan: '>',
  GreaterThanOrEqual: '>=',
  LessThan: '<',
  LessThanOrEqual: '<=',
  NotEquals: '!=',
}

export const METRIC_TRANSLATIONS: Record<BadgeMetric, TranslationKey> = {
  ScorePercentage: 'badges.metrics.scorePercentage',
  TotalScore: 'badges.metrics.totalScore',
  PerfectAnswersCount: 'badges.metrics.perfectAnswers',
  TotalAttempts: 'badges.metrics.totalAttempts',
}
