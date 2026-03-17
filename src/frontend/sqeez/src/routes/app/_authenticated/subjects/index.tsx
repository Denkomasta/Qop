import { createFileRoute } from '@tanstack/react-router'
import { EnrollmentsView } from './-/EnrollmentsView'

export const Route = createFileRoute('/app/_authenticated/subjects/')({
  component: EnrollmentsView,
})
