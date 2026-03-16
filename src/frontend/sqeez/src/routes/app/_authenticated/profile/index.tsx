import { createFileRoute } from '@tanstack/react-router'
import { ProfileView } from './-/ProfileView'

export const Route = createFileRoute('/app/_authenticated/profile/')({
  component: MyProfileRoute,
})

function MyProfileRoute() {
  return <ProfileView />
}
