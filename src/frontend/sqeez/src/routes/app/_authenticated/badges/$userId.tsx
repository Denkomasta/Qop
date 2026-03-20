import { createFileRoute } from '@tanstack/react-router'
import { BadgesView } from './-/BadgesView'

export const Route = createFileRoute('/app/_authenticated/badges/$userId')({
  component: BadgesRoute,
})

function BadgesRoute() {
  const { userId } = Route.useParams()

  return <BadgesView targetUserId={Number(userId)} />
}
