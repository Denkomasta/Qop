import { Navbar } from '@/components/layouting/Navbar/Navbar'
import { ThemeProvider } from '@/context/ThemeContext'
import { useAuthStore } from '@/store/useAuthStore'
import { createRootRoute, Outlet } from '@tanstack/react-router'
import { TanStackRouterDevtools } from '@tanstack/react-router-devtools'
import { useTranslation } from 'react-i18next'

const RootLayout = () => {
  const { t } = useTranslation()
  const { isAuthenticated, user } = useAuthStore()

  const navLinks = [{ to: '/', label: t('common.home') }]

  return (
    <ThemeProvider>
      <Navbar
        navLinks={navLinks}
        title={t('system.name')}
        loginButtonText={t('common.signIn')}
        registerButtonText={t('common.register')}
        navigationText={t('navbar.navigation')}
        logoutButtonText={t('navbar.logout')}
        isAuthenticated={isAuthenticated}
        username={user?.username || ''}
      />
      <Outlet />
      <TanStackRouterDevtools />
    </ThemeProvider>
  )
}

export const Route = createRootRoute({ component: RootLayout })
