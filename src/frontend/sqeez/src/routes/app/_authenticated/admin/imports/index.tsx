import { createFileRoute } from '@tanstack/react-router'
import { AdminImportPage } from './-/AdminImportPage'

export const Route = createFileRoute('/app/_authenticated/admin/imports/')({
  component: RouteComponent,
})

function RouteComponent() {
  return <AdminImportPage />
}
