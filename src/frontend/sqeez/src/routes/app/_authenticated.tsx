import { createFileRoute, Outlet, redirect } from '@tanstack/react-router'
import { useAuthStore } from '@/store/useAuthStore'
import { queryClient } from '@/main'
import { postApiAuthMe } from '@/api/generated/endpoints/auth/auth'

export const Route = createFileRoute('/app/_authenticated')({
  beforeLoad: async ({ location }) => {
    const { isAuthenticated, setUser } = useAuthStore.getState()

    if (!isAuthenticated) {
      try {
        const user = await queryClient.fetchQuery({
          queryKey: ['postApiAuthMe'],
          queryFn: () => postApiAuthMe(),
          staleTime: 1000 * 60 * 5,
        })

        if (user) {
          setUser(user)
          return
        }
      } catch {
        throw redirect({
          to: '/login',
          search: { redirect: location.href },
        })
      }
    }
  },
  component: () => <Outlet />,
})
