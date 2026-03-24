import { createFileRoute, Outlet } from '@tanstack/react-router'
import { teacherRouteGuard } from '@/lib/routeGuards'

export const Route = createFileRoute('/app/_authenticated/teacher')({
  beforeLoad: teacherRouteGuard,
  component: () => <Outlet />,
})
