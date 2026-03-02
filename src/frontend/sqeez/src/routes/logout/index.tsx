import { useState } from 'react'
import { createFileRoute, useNavigate, Link } from '@tanstack/react-router'
import { useAuthStore } from '@/store/useAuthStore'
import { useQueryClient } from '@tanstack/react-query'
import { Button } from '@/components/ui/Button'
import { LogOut, ArrowLeft, Loader2 } from 'lucide-react'
import { postApiAuthLogout } from '@/api/generated/endpoints/auth/auth'
import { useTranslation } from 'react-i18next'

export const Route = createFileRoute('/logout/')({
  component: LogoutPage,
})

function LogoutPage() {
  const navigate = useNavigate()
  const { t } = useTranslation()
  const logout = useAuthStore((s) => s.logout)
  const queryClient = useQueryClient()
  const [isPending, setIsPending] = useState(false)

  const handleConfirmLogout = async () => {
    setIsPending(true)

    try {
      // TODO add toasts
      await postApiAuthLogout()
    } catch (error) {
      console.error(
        'Server logout failed, but clearing local session anyway',
        error,
      )
    } finally {
      logout()

      queryClient.clear()

      navigate({ to: '/', replace: true })
    }
  }

  return (
    <div className="flex min-h-[80vh] flex-col items-center justify-center p-4">
      <div className="w-full max-w-sm rounded-2xl border bg-card p-8 text-center shadow-sm">
        <div className="mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-destructive/10 text-destructive">
          <LogOut className="h-6 w-6" />
        </div>

        <h1 className="text-2xl font-bold tracking-tight">
          {t('logout.title')}
        </h1>
        <p className="mt-2 text-muted-foreground">{t('logout.description')}</p>

        <div className="mt-8 flex flex-col gap-3">
          <Button
            variant="destructive"
            className="h-11 w-full cursor-pointer rounded-xl font-semibold"
            onClick={handleConfirmLogout}
            disabled={isPending}
          >
            {isPending ? (
              <>
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                {t('logout.signingOut')}
              </>
            ) : (
              t('logout.confirm')
            )}
          </Button>

          <Button
            variant="ghost"
            className="h-11 w-full rounded-xl"
            disabled={isPending}
            asChild
          >
            <Link to="/app">
              <ArrowLeft className="mr-2 h-4 w-4" />
              {t('common.goBack')}
            </Link>
          </Button>
        </div>
      </div>
    </div>
  )
}
