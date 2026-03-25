import { createFileRoute } from '@tanstack/react-router'

import { useQuizEditorUIStore } from '@/store/useQuizEditorUIStore'
import { useEffect } from 'react'
import { QuizEditorSidebar } from './-/QuizEditorSidebar'
import { QuizQuestionEditor } from './-/QuizQuestionEditor'
import { teacherRouteGuard } from '@/lib/routeGuards'

export const Route = createFileRoute(
  '/app/_authenticated/quizzes/$quizId/builder/',
)({
  beforeLoad: teacherRouteGuard,
  component: QuizEditorPage,
})

export function QuizEditorPage() {
  const { actions } = useQuizEditorUIStore()
  const { quizId } = Route.useParams()

  useEffect(() => {
    return () => actions.resetEditor()
  }, [actions])

  return (
    <div className="flex w-full flex-1 flex-col overflow-hidden bg-background">
      <div className="flex flex-1 overflow-hidden">
        <QuizEditorSidebar quizId={quizId} />

        <QuizQuestionEditor quizId={quizId} />
      </div>
    </div>
  )
}
