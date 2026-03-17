import { createFileRoute } from '@tanstack/react-router'
import { ClassView } from './-/ClassView'

export const Route = createFileRoute('/app/_authenticated/class/')({
  component: MyClassRoute,
})

function MyClassRoute() {
  return <ClassView />
}
