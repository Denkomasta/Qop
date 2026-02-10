import { Navbar } from '@/components/layouting/Navbar/Navbar'
import { createRootRoute, Outlet } from '@tanstack/react-router'
import { TanStackRouterDevtools } from '@tanstack/react-router-devtools'
import { useTranslation } from 'react-i18next'

const RootLayout = () => {
  const { t } = useTranslation()

  const navLinks = [{ to: '/', label: t('common.home') }]

  return (
    <>
      <Navbar
        navLinks={navLinks}
        title="Sqeez"
        loginButtonText={t('common.signIn')}
        registerButtonText={t('common.register')}
        navigationText={t('navbar.navigation')}
      />
      <Outlet />
      <TanStackRouterDevtools />
    </>
  )
}

export const Route = createRootRoute({ component: RootLayout })
