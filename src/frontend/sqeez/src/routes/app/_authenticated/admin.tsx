import { adminRouteGuard } from '@/lib/routeGuards'
import { createFileRoute, Outlet } from '@tanstack/react-router'

export const Route = createFileRoute('/app/_authenticated/admin')({
  beforeLoad: adminRouteGuard,
  component: () => <Outlet />,
})
