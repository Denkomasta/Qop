import { createFileRoute } from '@tanstack/react-router'
import { AdminClassDetailsPage } from './-/AdminClassDetailsPage'

export const Route = createFileRoute(
  '/app/_authenticated/admin/classes/$classId',
)({
  component: RouteComponent,
})

function RouteComponent() {
  const { classId } = Route.useParams()

  return <AdminClassDetailsPage classId={classId} />
}
