import { useTranslation } from 'react-i18next'
import { Link } from '@tanstack/react-router'
import { Eye, CheckCircle2, XCircle } from 'lucide-react'

import { DataTable, type ColumnDef } from '@/components/ui/Table/DataTable'
import { SimpleAvatar } from '@/components/ui/Avatar'
import { Badge } from '@/components/ui/Badge/Badge'
import { Button } from '@/components/ui/Button'
import { getImageUrl } from '@/lib/imageHelpers'
import { formatName } from '@/lib/userHelpers'

// Replace 'any' with your actual generated DTO (e.g., QuizAttemptDto)
interface QuizAttemptsTableProps {
  attempts: any[]
  isLoading?: boolean
}

export function QuizAttemptsTable({
  attempts,
  isLoading = false,
}: QuizAttemptsTableProps) {
  const { t } = useTranslation()

  const columns: ColumnDef<any>[] = [
    {
      header: t('common.student', 'Student'),
      cell: (attempt) => (
        <div className="flex items-center gap-3">
          <SimpleAvatar
            url={getImageUrl(attempt.studentAvatarUrl)}
            firstName={attempt.studentFirstName}
            lastName={attempt.studentLastName}
            wrapperClassName="size-8 shrink-0"
          />
          <div className="flex flex-col">
            <span className="font-medium text-foreground">
              {formatName(attempt.studentFirstName, attempt.studentLastName)}
            </span>
            <span className="text-xs text-muted-foreground">
              {attempt.studentEmail || attempt.studentUsername}
            </span>
          </div>
        </div>
      ),
    },
    {
      header: t('quiz.submittedAt', 'Submitted'),
      cell: (attempt) => (
        <span className="text-muted-foreground">
          {attempt.submittedAt ? (
            new Date(attempt.submittedAt).toLocaleString()
          ) : (
            <span className="italic">
              {t('quiz.inProgress', 'In Progress')}
            </span>
          )}
        </span>
      ),
    },
    {
      header: t('quiz.status', 'Status'),
      cell: (attempt) => (
        <Badge
          variant={attempt.isGraded ? 'default' : 'secondary'}
          className="shadow-none"
        >
          {attempt.isGraded
            ? t('quiz.graded', 'Graded')
            : t('quiz.pendingGrade', 'Pending')}
        </Badge>
      ),
    },
    {
      header: t('quiz.score', 'Score'),
      cell: (attempt) => {
        const isPassed = attempt.score >= (attempt.passMark || 50)

        if (!attempt.submittedAt) {
          return <span className="text-muted-foreground">-</span>
        }

        return (
          <div className="flex items-center gap-2">
            {isPassed ? (
              <CheckCircle2 className="h-4 w-4 text-emerald-500" />
            ) : (
              <XCircle className="h-4 w-4 text-destructive" />
            )}
            <span
              className={`font-bold ${
                isPassed ? 'text-emerald-600' : 'text-destructive'
              }`}
            >
              {attempt.score}%
            </span>
          </div>
        )
      },
    },
    {
      header: '',
      className: 'text-right w-[120px]',
      cell: (attempt) => (
        <div className="flex justify-end gap-2">
          <Link
            to="/app/quizzes/attempts/$attemptId"
            params={{ attemptId: attempt.id.toString() }}
          >
            <Button
              variant="ghost"
              size="sm"
              className="gap-2 text-primary hover:bg-primary/10"
            >
              <Eye className="h-4 w-4" />
              <span className="hidden sm:inline">{t('quiz.review')}</span>
            </Button>
          </Link>
        </div>
      ),
    },
  ]

  return (
    <DataTable
      data={attempts}
      columns={columns}
      isLoading={isLoading}
      emptyMessage={t('quiz.noAttemptsYet')}
      keyExtractor={(attempt) => attempt.id ?? 'unknown'}
    />
  )
}
