import { useState } from 'react'
import { Eye, EyeOff, Mail, Lock, BookOpen } from 'lucide-react'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { Label } from '@/components/ui/Label'
import { Checkbox } from '@/components/ui/Checkbox'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate } from '@tanstack/react-router'

export function RegisterForm() {
  const navigate = useNavigate()
  const { t } = useTranslation()
  const [showPassword, setShowPassword] = useState(false)
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    // TODO: Implement login logic
    navigate({ to: '/dashboard' })
  }

  return (
    <div className="flex w-full max-w-md flex-col gap-8">
      <div className="flex items-center gap-2.5 lg:hidden">
        <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-primary">
          <BookOpen
            className="h-5 w-5 text-primary-foreground"
            aria-hidden="true"
          />
        </div>
        <span className="text-xl font-bold tracking-tight text-foreground">
          {t('system.name')}
        </span>
      </div>

      <div className="flex flex-col gap-2">
        <h1 className="text-3xl font-bold tracking-tight text-balance text-foreground">
          {t('register.title')}
        </h1>
        <p className="leading-relaxed text-muted-foreground">
          {t('register.subtitle')}
        </p>
      </div>

      <form onSubmit={handleSubmit} className="flex flex-col gap-5">
        <div className="flex flex-col gap-2">
          <Label
            htmlFor="email"
            className="text-sm font-medium text-foreground"
          >
            {t('login.email')}
          </Label>
          <div className="relative">
            <Mail
              className="absolute top-1/2 left-3 h-4 w-4 -translate-y-1/2 text-muted-foreground"
              aria-hidden="true"
            />
            <Input
              id="email"
              type="email"
              placeholder="name@example.com"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="h-11 rounded-xl border-border bg-card pl-10 text-foreground placeholder:text-muted-foreground focus-visible:ring-ring"
              required
              autoComplete="email"
            />
          </div>
        </div>

        <div className="flex flex-col gap-2">
          <div className="flex items-center justify-between">
            <Label
              htmlFor="password"
              className="text-sm font-medium text-foreground"
            >
              {t('login.password')}
            </Label>
            <a // TODO: Link to forgot password page
              href=""
              className="text-xs font-medium text-primary transition-colors hover:text-primary/80"
            >
              {t('login.forgotPassword')}
            </a>
          </div>
          <div className="relative">
            <Lock
              className="absolute top-1/2 left-3 h-4 w-4 -translate-y-1/2 text-muted-foreground"
              aria-hidden="true"
            />
            <Input
              id="password"
              type={showPassword ? 'text' : 'password'}
              placeholder={t('login.password')}
              value={password}
              onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                setPassword(e.target.value)
              }
              className="h-11 rounded-xl border-border bg-card pr-10 pl-10 text-foreground placeholder:text-muted-foreground focus-visible:ring-ring"
              required
              autoComplete="current-password"
            />
            <button
              type="button"
              onClick={() => setShowPassword(!showPassword)}
              className="absolute top-1/2 right-3 -translate-y-1/2 text-muted-foreground transition-colors hover:text-foreground"
              aria-label={
                showPassword ? t('login.hidePassword') : t('login.showPassword')
              }
            >
              {showPassword ? (
                <EyeOff className="h-4 w-4" />
              ) : (
                <Eye className="h-4 w-4" />
              )}
            </button>
          </div>
        </div>

        <div className="flex items-center gap-2">
          <Checkbox id="remember" aria-label="Remember me for 30 days" />
          <Label
            htmlFor="remember"
            className="cursor-pointer text-sm font-normal text-muted-foreground"
          >
            {t('login.rememberMe')}
          </Label>
        </div>

        <Button
          type="submit"
          className="h-11 w-full rounded-xl bg-primary font-semibold text-primary-foreground transition-all hover:bg-primary/90 active:scale-[0.98]"
        >
          {t('common.signIn')}
        </Button>
      </form>

      <p className="text-center text-sm text-muted-foreground">
        {t('register.alreadyHaveAccount')}{' '}
        <Link
          to="/login"
          className="font-semibold text-primary transition-colors hover:text-primary/80"
        >
          {t('register.signIn')}
        </Link>
      </p>
    </div>
  )
}
