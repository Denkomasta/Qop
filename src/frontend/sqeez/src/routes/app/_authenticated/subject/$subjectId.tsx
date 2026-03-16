import { createFileRoute } from '@tanstack/react-router'

export const Route = createFileRoute('/app/_authenticated/subject/$subjectId')({
  component: RouteComponent,
})

function RouteComponent() {
  const { subjectId } = Route.useParams()

  return <div>Hello "/app/_authenticated/subject/{subjectId}"!</div>
}
