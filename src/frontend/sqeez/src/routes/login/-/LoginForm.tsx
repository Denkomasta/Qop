import { Mail, Lock, BookOpen, Loader2 } from 'lucide-react'
import { Button } from '@/components/ui'
import { Input } from '@/components/ui/Input'
import { Label } from '@/components/ui/Label'
import { Checkbox } from '@/components/ui/Checkbox'
import { useTranslation } from 'react-i18next'
import { Link, useSearch } from '@tanstack/react-router'
import { usePostApiAuthLogin as useLogin } from '@/api/generated/endpoints/auth/auth'
import { zodResolver } from '@hookform/resolvers/zod'
import * as z from 'zod'
import { useForm, Controller, type SubmitHandler } from 'react-hook-form'
import type { TranslationKey } from '@/i18next'
import { AxiosError } from 'axios'
import { useAuthSuccess } from '@/hooks/useAuthSuccess'

const loginSchema = z.object({
  email: z.email({ message: 'Invalid email address' }),
  password: z
    .string()
    .min(4, { message: 'Password must be at least 4 characters' }),
  remember: z.boolean(),
})

const errorMapping: Record<number, TranslationKey> = {
  401: 'error.invalidCredentials',
  500: 'error.serverError',
}

type LoginFormValues = z.infer<typeof loginSchema>

export function LoginForm() {
  const { t } = useTranslation()
  const search = useSearch({ strict: false })
  const handleAuthSuccess = useAuthSuccess()

  const {
    register,
    handleSubmit,
    setError,
    control,
    formState: { errors },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: '',
      password: '',
      remember: false,
    },
  })

  const { mutate, isPending } = useLogin({
    mutation: {
      onSuccess: async () => {
        try {
          await handleAuthSuccess(search?.redirect)
        } catch {
          setError('root', {
            type: 'manual',
            message: t('error.serverError'),
          })
        }
      },
      onError: (error: AxiosError) => {
        setError('root', {
          type: 'manual',
          message: t(
            error.response?.status
              ? errorMapping[error.response.status]
              : 'error.generic',
          ),
        })
      },
    },
  })

  const onSubmit: SubmitHandler<LoginFormValues> = (values) => {
    mutate({
      data: {
        email: values.email,
        password: values.password,
        rememberMe: values.remember,
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

      <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-2">
        <Input
          {...register('email')}
          id="email"
          type="email"
          label={t('login.email')}
          placeholder={t('login.emailPlaceholder')}
          error={errors.email?.message}
          disabled={isPending}
          icon={<Mail className="h-4 w-4" />}
        />

        <Input
          {...register('password')}
          id="password"
          type="password"
          label={t('login.password')}
          placeholder={t('login.password')}
          error={errors.password?.message}
          disabled={isPending}
          icon={<Lock className="h-4 w-4" />}
          rightTopChip={
            <>
              <a // TODO: Link to forgot password page
                href="#"
                className="text-xs font-medium text-primary transition-colors hover:text-primary/80"
              >
                {t('login.forgotPassword')}
              </a>
            </>
          }
        />

        <div className="flex items-center gap-2">
          <Controller
            name="remember"
            control={control}
            render={({ field }) => (
              <Checkbox
                id="remember"
                checked={field.value}
                onCheckedChange={field.onChange}
                disabled={isPending}
                aria-label="Remember me for 30 days"
              />
            )}
          />
          <Label
            htmlFor="remember"
            className="cursor-pointer text-sm font-normal text-muted-foreground"
          >
            {t('login.rememberMe')}
          </Label>
        </div>

        {errors.root && (
          <div className="animate-in rounded-lg bg-destructive/15 p-3 text-[0.8rem] font-medium text-destructive ring-1 ring-destructive/20 transition-all zoom-in-95 fade-in">
            {errors.root.message}
          </div>
        )}

        <Button
          type="submit"
          className="h-11 w-full rounded-xl bg-primary font-semibold text-primary-foreground transition-all hover:bg-primary/90 active:scale-[0.98]"
          disabled={isPending}
        >
          {isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
          {t('common.signIn')}
        </Button>

        <p className="text-center text-sm text-muted-foreground">
          {t('login.areYouNew')}{' '}
          <Link
            to="/register"
            className="font-semibold text-primary transition-colors hover:text-primary/80"
          >
            {t('login.createAccount')}
          </Link>
        </p>
      </form>
    </div>
  )
}
