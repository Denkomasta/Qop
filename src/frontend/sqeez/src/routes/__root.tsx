import { Navbar } from '@/components/layouting/Navbar/Navbar'
import { ThemeProvider } from '@/context/ThemeContext'
import { createRootRoute, Outlet } from '@tanstack/react-router'
import { TanStackRouterDevtools } from '@tanstack/react-router-devtools'
import { useTranslation } from 'react-i18next'

const RootLayout = () => {
  const { t } = useTranslation()

  const navLinks = [{ to: '/', label: t('common.home') }]

  return (
    <ThemeProvider>
      <Navbar
        navLinks={navLinks}
        title="Sqeez"
        loginButtonText={t('common.signIn')}
        registerButtonText={t('common.register')}
        navigationText={t('navbar.navigation')}
      />
      <Outlet />
      <TanStackRouterDevtools />
    </ThemeProvider>
  )
}

export const Route = createRootRoute({ component: RootLayout })
