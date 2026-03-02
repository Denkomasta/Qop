import { useState } from 'react'
import { Eye, EyeOff, Mail, Lock, BookOpen, Loader2 } from 'lucide-react'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { Label } from '@/components/ui/Label'
import { Checkbox } from '@/components/ui/Checkbox'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate } from '@tanstack/react-router'
import { usePostApiAuthLogin as useLogin } from '@/api/generated/endpoints/auth/auth'
import { zodResolver } from '@hookform/resolvers/zod'
import * as z from 'zod'
import { useForm } from 'react-hook-form'

const loginSchema = z.object({
  email: z.email({ message: 'Invalid email address' }),
  password: z
    .string()
    .min(4, { message: 'Password must be at least 4 characters' }),
})

type LoginFormValues = z.infer<typeof loginSchema>

export function LoginForm() {
  const navigate = useNavigate()
  const { t } = useTranslation()
  const [showPassword, setShowPassword] = useState(false)

  const { mutate, isPending } = useLogin({
    mutation: {
      onSuccess: () => {
        navigate({ to: '/app' })
      },
    },
  })

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: '',
      password: '',
    },
  })

  const onSubmit = (values: LoginFormValues) => {
    mutate({
      data: {
        email: values.email,
        password: values.password,
      },
    })
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
          {t('login.welcomeBack')}
        </h1>
        <p className="leading-relaxed text-muted-foreground">
          {t('login.signInToYourAccount')}
        </p>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-5">
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
              {...register('email')}
              id="email"
              type="email"
              placeholder="name@example.com"
              className="h-11 rounded-xl border-border bg-card pl-10 text-foreground placeholder:text-muted-foreground focus-visible:ring-ring"
              disabled={isPending}
            />
          </div>
          {errors.email && (
            <span className="text-xs text-destructive">
              {errors.email.message}
            </span>
          )}
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
              href="#"
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
              {...register('password')}
              id="password"
              type={showPassword ? 'text' : 'password'}
              placeholder={t('login.password')}
              className="h-11 rounded-xl border-border bg-card pr-10 pl-10 text-foreground placeholder:text-muted-foreground focus-visible:ring-ring"
              disabled={isPending}
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
          {errors.password && (
            <span className="text-xs text-destructive">
              {errors.password.message}
            </span>
          )}
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
          disabled={isPending}
        >
          {isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
          {t('common.signIn')}
        </Button>
      </form>

      <p className="text-center text-sm text-muted-foreground">
        {t('login.areYouNew')}{' '}
        <Link
          to="/register"
          className="font-semibold text-primary transition-colors hover:text-primary/80"
        >
          {t('login.createAccount')}
        </Link>
      </p>
    </div>
  )
}
