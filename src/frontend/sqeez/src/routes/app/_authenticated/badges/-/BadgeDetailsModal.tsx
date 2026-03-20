import { useTranslation } from 'react-i18next'
import { Star, Shield, Target } from 'lucide-react'
import { BaseModal } from '@/components/ui/Modal'
import { getImageUrl } from '@/lib/imageHelpers'
import type {
  BadgeDto,
  BadgeOperator,
  BadgeMetric,
} from '@/api/generated/model'

interface BadgeDetailsModalProps {
  isOpen: boolean
  onClose: () => void
  badge: BadgeDto | null
  isEarned: boolean
  earnedDate?: string
}

const OPERATOR_MAP: Record<BadgeOperator, string> = {
  Equals: '=',
  GreaterThan: '>',
  GreaterThanOrEqual: '>=',
  LessThan: '<',
  LessThanOrEqual: '<=',
  NotEquals: '!=',
}

export function BadgeDetailsModal({
  isOpen,
  onClose,
  badge,
  isEarned,
  earnedDate,
}: BadgeDetailsModalProps) {
  const { t } = useTranslation()

  if (!badge) return null

  const getReadableMetric = (metric: BadgeMetric) => {
    const metricTranslations: Record<BadgeMetric, string> = {
      ScorePercentage: t('badges.metrics.scorePercentage', 'Score Percentage'),
      TotalScore: t('badges.metrics.totalScore', 'Total Score'),
      PerfectAnswersCount: t(
        'badges.metrics.perfectAnswers',
        'Perfect Answers',
      ),
      TotalAttempts: t('badges.metrics.totalAttempts', 'Total Attempts'),
    }
    return metricTranslations[metric] || metric
  }

  return (
    <BaseModal isOpen={isOpen} onClose={onClose} title={badge.name}>
      <div className="flex flex-col items-center gap-6 pt-4 pb-2">
        <div
          className={`flex h-32 w-32 items-center justify-center rounded-full p-4 ${
            isEarned ? 'bg-primary/10' : 'bg-muted grayscale'
          }`}
        >
          {badge.iconUrl ? (
            <img
              src={getImageUrl(badge.iconUrl)}
              alt={badge.name}
              className="h-full w-full object-contain"
            />
          ) : (
            <Shield className="h-16 w-16 text-primary" />
          )}
        </div>

        <div className="flex flex-col items-center gap-2">
          <div className="flex items-center gap-1.5 rounded-full bg-yellow-500/10 px-3 py-1 text-sm font-bold text-yellow-600 dark:text-yellow-500">
            <Star className="h-4 w-4 fill-yellow-500" />+{badge.xpBonus} XP
          </div>
          {isEarned ? (
            <span className="text-sm font-medium text-green-600 dark:text-green-400">
              {t('badges.earnedOn', 'Earned on')}{' '}
              {earnedDate ? new Date(earnedDate).toLocaleDateString() : ''}
            </span>
          ) : (
            <span className="text-sm font-medium text-muted-foreground">
              {t('badges.notEarned', 'Not earned yet')}
            </span>
          )}
        </div>

        <p className="text-center text-sm text-foreground">
          {badge.description}
        </p>

        {badge.rules && badge.rules.length > 0 && (
          <div className="w-full rounded-lg border bg-muted/30 p-4">
            <h4 className="mb-3 flex items-center gap-2 text-sm font-semibold text-foreground">
              <Target className="h-4 w-4" />
              {t('badges.requirements', 'Requirements')}
            </h4>
            <ul className="flex flex-col gap-2">
              {badge.rules.map((rule) => {
                const isPercentage = rule.metric === 'ScorePercentage'
                const displayValue = isPercentage
                  ? `${rule.targetValue}%`
                  : rule.targetValue

                return (
                  <li
                    key={rule.id}
                    className="flex items-start gap-2 text-sm text-muted-foreground"
                  >
                    <div className="mt-1.5 h-1.5 w-1.5 shrink-0 rounded-full bg-primary" />
                    <span>
                      <span className="font-medium text-foreground">
                        {getReadableMetric(rule.metric)}
                      </span>{' '}
                      <span className="mx-1 font-bold text-primary">
                        {OPERATOR_MAP[rule.operator]}
                      </span>{' '}
                      <span className="font-medium text-foreground">
                        {displayValue}
                      </span>
                    </span>
                  </li>
                )
              })}
            </ul>
          </div>
        )}
      </div>
    </BaseModal>
  )
}
