import { createFileRoute } from '@tanstack/react-router'
import { AdminSubjectsPage } from './-/AdminSubjectPage'

export const Route = createFileRoute('/app/_authenticated/admin/subjects/')({
  component: AdminSubjectsPage,
})
