import { Footer } from '@/components/layouting/Footer/Footer'
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
      <div className="flex min-h-screen flex-col bg-background">
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

        <main className="flex-1">
          <Outlet />
        </main>

        <Footer />
      </div>
      <TanStackRouterDevtools />
    </ThemeProvider>
  )
}

export const Route = createRootRoute({ component: RootLayout })
