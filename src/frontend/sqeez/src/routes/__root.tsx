import { useGetApiAuthMe } from '@/api/generated/endpoints/auth/auth'
import { Footer } from '@/components/layouting/Footer/Footer'
import { Navbar } from '@/components/layouting/Navbar/Navbar'
import { Toaster } from '@/components/ui/Sonner'
import { Spinner } from '@/components/ui/Spinner'
import { ThemeProvider } from '@/context/ThemeContext'
import { useSystemConfig } from '@/hooks/useSystemConfig'
import { useAuthStore } from '@/store/useAuthStore'
import { createRootRoute, Outlet } from '@tanstack/react-router'
import { TanStackRouterDevtools } from '@tanstack/react-router-devtools'
import { useEffect } from 'react'
import { useTranslation } from 'react-i18next'

const RootLayout = () => {
  const { t } = useTranslation()
  const { setUser, logout } = useAuthStore()
  const currentYear = new Date().getFullYear()

  const {
    data: user,
    error,
    isLoading,
  } = useGetApiAuthMe({
    query: {
      retry: false,
      staleTime: 1000 * 60 * 5,
      refetchOnWindowFocus: false,
    },
  })

  const { config, isLoading: isSystemConfigLoading } = useSystemConfig()

  useEffect(() => {
    if (user) {
      setUser(user)
    } else if (error != null) {
      if (error?.response?.status === 401) {
        logout()
      }
    }
  }, [user, error, setUser, logout])

  const navLinks = [{ to: '/', label: t('common.home') }]
  const footerLinks = [
    { to: '/help', label: t('footer.help') },
    { to: '/privacy', label: t('footer.privacy') },
    { to: '/terms', label: t('footer.terms') },
  ]

  if (isLoading || isSystemConfigLoading) {
    return (
      <div className="flex min-h-[60vh] flex-col items-center justify-center gap-4">
        <Spinner size="lg" />
        <p className="animate-pulse font-medium text-muted-foreground">
          {t('common.loading')}...
        </p>
      </div>
    )
  }

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
          isAuthenticated={!!user}
          user={user}
          isRegisterEnabled={config?.allowPublicRegistration ?? false}
        />

        <main className="flex flex-1 flex-col">
          <Outlet />
        </main>

        <Footer
          links={footerLinks}
          rightsText={`© ${currentYear} ${t('system.name')}. ${t('footer.rights')}`}
        />
      </div>
      <Toaster richColors position="bottom-right" />
      <TanStackRouterDevtools />
    </ThemeProvider>
  )
}

export const Route = createRootRoute({ component: RootLayout })
