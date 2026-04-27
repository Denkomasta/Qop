import { createFileRoute } from '@tanstack/react-router'
import { SubjectDetailsPage } from '../../../subjects/$subjectId/students/-/SubjectDetailsPage'

export const Route = createFileRoute(
  '/app/_authenticated/teacher/subjects/$subjectId/',
)({
  component: OtherClassRoute,
})

function OtherClassRoute() {
  const { subjectId } = Route.useParams()

  return <SubjectDetailsPage subjectId={Number(subjectId)} />
}
