import { createFileRoute } from '@tanstack/react-router'
import { AdminSchoolClassPage } from './-/AdminSchoolClassPage'

export const Route = createFileRoute('/app/_authenticated/admin/classes/')({
  component: AdminSchoolClassPage,
})
