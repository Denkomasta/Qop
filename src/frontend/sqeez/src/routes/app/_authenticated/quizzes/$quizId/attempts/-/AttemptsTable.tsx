import { Link } from '@tanstack/react-router'
import { useTranslation } from 'react-i18next'
import { Eye, PenTool, CheckCircle2, Clock } from 'lucide-react'

import { Badge } from '@/components/ui/Badge'
import { Button } from '@/components/ui/Button'
import { DataTable, type ColumnDef } from '@/components/ui/Table'

export interface AttemptRowDto {
  id: number | string
  quizId: number | string
  quizTitle?: string
  studentName?: string
  status: 'InProgress' | 'PendingGrading' | 'Completed' | string
  totalScore: number
  startTime: string
}

interface AttemptsTableProps {
  attempts: AttemptRowDto[]
  isTeacherView: boolean
  isLoading?: boolean
  isQuizActive?: boolean
}

export function AttemptsTable({
  attempts,
  isTeacherView,
  isLoading,
  isQuizActive = false,
}: AttemptsTableProps) {
  const { t } = useTranslation()

  const columns: ColumnDef<AttemptRowDto>[] = [
    {
      header: isTeacherView ? t('common.student') : t('common.quiz'),
      cell: (item) => (
        <span className="font-medium">
          {isTeacherView ? item.studentName : item.quizTitle}
        </span>
      ),
    },
    {
      header: t('common.date'),
      cell: (item) => new Date(item.startTime).toLocaleDateString(),
      className: 'text-muted-foreground',
    },
    {
      header: t('attempts.status'),
      cell: (item) => {
        const isNeedsGrading =
          item.status === 'PendingGrading' || item.status === 'NeedsGrading'
        const isCompleted = item.status === 'Completed'

        if (isNeedsGrading) {
          return (
            <Badge
              variant="outline"
              className="border-yellow-500/50 bg-yellow-500/10 text-yellow-600"
            >
              <Clock className="mr-1 h-3 w-3" />
              {t('grading.needsGrading')}
            </Badge>
          )
        }
        if (isCompleted) {
          return (
            <Badge
              variant="outline"
              className="border-green-500/50 bg-green-500/10 text-green-600"
            >
              <CheckCircle2 className="mr-1 h-3 w-3" />
              {t('attempts.completed')}
            </Badge>
          )
        }
        return <Badge variant="secondary">{item.status}</Badge>
      },
    },
    {
      header: t('common.score'),
      className: 'text-right',
      cell: (item) => (
        <span className="font-bold">
          {t('common.points')}: {item.totalScore}
        </span>
      ),
    },
    {
      header: t('common.actions'),
      className: 'w-[100px] text-center',
      cell: (item) => {
        const isNeedsGrading =
          item.status === 'PendingGrading' || item.status === 'NeedsGrading'
        const showGradeButton = isTeacherView && isNeedsGrading
        const isViewDisabled = !isTeacherView && isQuizActive

        return (
          <Link
            to={`/app/quizzes/$quizId/attempts/$attemptId`}
            params={{ quizId: String(item.quizId), attemptId: String(item.id) }}
            disabled={isViewDisabled}
            className={isViewDisabled ? 'pointer-events-none opacity-50' : ''}
          >
            <Button
              variant={showGradeButton ? 'default' : 'ghost'}
              size="sm"
              disabled={isViewDisabled}
              className={
                showGradeButton
                  ? 'bg-yellow-500 text-white hover:bg-yellow-600'
                  : ''
              }
            >
              {showGradeButton ? (
                <>
                  <PenTool className="mr-2 h-4 w-4" />
                  {t('common.grade')}
                </>
              ) : (
                <>
                  <Eye className="mr-2 h-4 w-4" />
                  {t('common.view')}
                </>
              )}
            </Button>
          </Link>
        )
      },
    },
  ]

  const emptyMessage = isTeacherView
    ? t('attempts.noAttemptsTeacher')
    : t('attempts.noAttemptsStudent')

  return (
    <DataTable
      data={attempts || []}
      columns={columns}
      isLoading={isLoading}
      emptyMessage={emptyMessage}
      keyExtractor={(item) => item.id}
    />
  )
}
