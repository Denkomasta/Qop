import { createFileRoute } from '@tanstack/react-router'
import { AdminBadgesPage } from './-/AdminBadgesPage'

export const Route = createFileRoute('/app/_authenticated/admin/badges/')({
  component: AdminBadgesPage,
})
