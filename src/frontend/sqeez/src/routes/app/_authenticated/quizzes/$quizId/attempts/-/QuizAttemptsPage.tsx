import { Link } from '@tanstack/react-router'
import { useTranslation } from 'react-i18next'
import { ArrowLeft, History, FileText } from 'lucide-react'

import { Button } from '@/components/ui/Button'
import { Spinner } from '@/components/ui/Spinner'

import { useAuthStore } from '@/store/useAuthStore'
import { useGetApiQuizzesQuizId } from '@/api/generated/endpoints/quizzes/quizzes'
import { useGetApiQuizAttemptsQuizQuizId } from '@/api/generated/endpoints/quiz-attempts/quiz-attempts'
import { AttemptsTable, type AttemptRowDto } from './AttemptsTable'
import { useState } from 'react'
import { Pagination } from '@/components/ui/Pagination'
import { isQuizActive } from '@/lib/quizHelpers'
import { useGetApiSubjectsId } from '@/api/generated/endpoints/subjects/subjects'

export function QuizAttemptsPage({ quizId }: { quizId: string }) {
  const { t } = useTranslation()
  const { user, isTeacher } = useAuthStore()

  const [pageNumber, setPageNumber] = useState(1)
  const PAGE_SIZE = 15

  const { data: quiz, isLoading: isQuizLoading } = useGetApiQuizzesQuizId(
    Number(quizId),
    { studentId: user?.id },
    { query: { enabled: !!quizId } },
  )

  const { data: pastAttempts, isLoading: isAttemptsLoading } =
    useGetApiQuizAttemptsQuizQuizId(
      Number(quizId),
      { pageNumber, pageSize: PAGE_SIZE },
      { query: { enabled: !!quizId && !!user?.id } },
    )

  const { data: subject, isLoading: isSubjectLoading } = useGetApiSubjectsId(
    Number(quiz?.subjectId),
    { query: { enabled: !!quiz?.subjectId && isTeacher } },
  )

  const isLoading = isQuizLoading || isAttemptsLoading || isSubjectLoading

  if (isLoading) {
    return (
      <div className="flex min-h-[60vh] flex-col items-center justify-center gap-4">
        <Spinner size="lg" />
      </div>
    )
  }

  const tableData: AttemptRowDto[] = (pastAttempts?.data || []).map(
    (attempt) => ({
      id: attempt.id,
      quizId: attempt.quizId,
      quizTitle: quiz?.title,
      status: String(attempt.status),
      totalScore: Number(attempt.totalScore || 0),
      studentName: attempt.studentName || '',
      studentId: attempt.studentId || undefined,
      startTime: attempt.startTime || new Date().toISOString(),
    }),
  )
  const totalPages = Number(pastAttempts?.totalPages || 1)
  const isActive = isQuizActive({
    publishDate: quiz?.publishDate,
    closingDate: quiz?.closingDate,
  })
  const isTeacherView = isTeacher && subject?.teacherId === user?.id

  return (
    <div className="container mx-auto max-w-5xl space-y-8 p-6">
      <div className="flex flex-col gap-6 border-b pb-8">
        <div>
          <Button
            variant="ghost"
            size="sm"
            asChild
            className="-ml-3 text-muted-foreground hover:text-foreground"
          >
            <Link to="/app/quizzes/$quizId" params={{ quizId }}>
              <ArrowLeft className="mr-2 size-4" />
              {t('common.backToQuiz', 'Back to Quiz')}
            </Link>
          </Button>
        </div>

        <div className="flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
          <div className="space-y-3">
            <div className="flex items-center gap-3">
              <div className="flex size-12 shrink-0 items-center justify-center rounded-xl bg-primary/10 text-primary">
                <History className="size-6" />
              </div>
              <div>
                <h1 className="text-3xl font-extrabold tracking-tight md:text-4xl">
                  {t('attempts.myResults', 'My Results')}
                </h1>
                <p className="mt-1 flex items-center gap-2 text-muted-foreground">
                  <FileText className="size-4" />
                  {quiz?.title}
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div className="space-y-4">
        <AttemptsTable
          attempts={tableData}
          isTeacherView={isTeacherView}
          isLoading={isAttemptsLoading}
          isQuizActive={isActive}
        />
      </div>

      {!isLoading && totalPages > 1 && (
        <div className="mt-6 flex justify-center">
          <Pagination
            currentPage={pageNumber}
            totalPages={totalPages}
            onPageChange={setPageNumber}
          />
        </div>
      )}
    </div>
  )
}
