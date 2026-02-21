import { createFileRoute } from '@tanstack/react-router'
import { LoginForm } from './-/LoginForm'
import { BrandingPanel } from '@/components/layouting/BrandingPanel'

export const Route = createFileRoute('/login/')({
  component: Login,
})

function Login() {
  return (
    <>
      <main className="flex min-h-screen">
        <div className="hidden lg:flex lg:w-1/2">
          <BrandingPanel />
        </div>

        <div className="flex w-full flex-col items-center justify-center bg-background px-6 py-12 lg:w-1/2 lg:px-16">
          <LoginForm />
        </div>
      </main>
    </>
  )
}
