import { Mail, Lock, BookOpen, Loader2 } from 'lucide-react'
import { Button } from '@/components/ui'
import { Input } from '@/components/ui/Input'
import { Label } from '@/components/ui/Label'
import { Checkbox } from '@/components/ui/Checkbox'
import { useTranslation } from 'react-i18next'
import { Link, useSearch } from '@tanstack/react-router'
import { useForm, Controller, type SubmitHandler } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import * as z from 'zod'
import { usePostApiAuthRegister as useRegister } from '@/api/generated/endpoints/auth/auth'
import { useAuthSuccess } from '@/hooks/useAuthSuccess' // Adjust path as needed

const registerSchema = z.object({
  username: z
    .string()
    .min(2, { message: 'Username must be at least 2 characters' }),
  email: z.email({ message: 'Invalid email address' }),
  password: z
    .string()
    .min(4, { message: 'Password must be at least 4 characters' }),
  remember: z.boolean(),
})

type RegisterFormValues = z.infer<typeof registerSchema>

export function RegisterForm() {
  const { t } = useTranslation()
  const search = useSearch({ strict: false })
  const handleAuthSuccess = useAuthSuccess()

  const {
    register,
    handleSubmit,
    setError,
    control,
    formState: { errors },
  } = useForm<RegisterFormValues>({
    resolver: zodResolver(registerSchema),
    defaultValues: {
      username: '',
      email: '',
      password: '',
      remember: false,
    },
  })

  const { mutate, isPending } = useRegister({
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
      onError: () => {
        setError('root', {
          type: 'manual',
          message: t('error.generic'),
        })
      },
    },
  })

  const onSubmit: SubmitHandler<RegisterFormValues> = (values) => {
    mutate({
      data: {
        username: values.username,
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
          {t('register.title')}
        </h1>
        <p className="leading-relaxed text-muted-foreground">
          {t('register.subtitle')}
        </p>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-2">
        <Input
          {...register('username')}
          id="username"
          type="text"
          label={t('register.username')}
          placeholder={t('register.username')}
          error={errors.username?.message}
          disabled={isPending}
        />

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
        />

        {/* Hooked up the Checkbox to React Hook Form using Controller */}
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
          {t('register.register')}
        </Button>

        <p className="text-center text-sm text-muted-foreground">
          {t('register.alreadyHaveAccount')}{' '}
          <Link
            to="/login"
            className="font-semibold text-primary transition-colors hover:text-primary/80"
          >
            {t('register.signIn')}
          </Link>
        </p>
      </form>
    </div>
  )
}
