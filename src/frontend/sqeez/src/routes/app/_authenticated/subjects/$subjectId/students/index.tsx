import { createFileRoute } from '@tanstack/react-router'
import { SubjectDetailsPage } from './-/SubjectDetailsPage'

export const Route = createFileRoute(
  '/app/_authenticated/subjects/$subjectId/students/',
)({
  component: RouteComponent,
})

function RouteComponent() {
  const { subjectId } = Route.useParams()

  return <SubjectDetailsPage subjectId={subjectId} />
}
