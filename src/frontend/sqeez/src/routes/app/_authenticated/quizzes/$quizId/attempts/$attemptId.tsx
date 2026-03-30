import { createFileRoute } from '@tanstack/react-router'
import { AttemptViewerPage } from './-/AttemptViewerPage'

export const Route = createFileRoute(
  '/app/_authenticated/quizzes/$quizId/attempts/$attemptId',
)({
  component: RouteComponent,
})

function RouteComponent() {
  const { quizId, attemptId } = Route.useParams()

  return <AttemptViewerPage quizId={quizId} attemptId={attemptId} />
}
