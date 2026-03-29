import { createFileRoute, Link } from '@tanstack/react-router'
import { Brain, Heart, Shield, Sparkles, Users } from 'lucide-react'
import { useTranslation } from 'react-i18next'

import { Button } from '@/components/ui/Button'

export const Route = createFileRoute('/about/')({
  component: About,
})

function About() {
  const { t } = useTranslation()

  return (
    <div className="flex min-h-screen flex-col bg-background text-foreground">
      <div className="flex-1">
        <section className="border-b border-border bg-secondary/20 py-20 sm:py-32">
          <div className="mx-auto max-w-7xl px-4 text-center sm:px-6 lg:px-8">
            <div className="mx-auto max-w-3xl space-y-6">
              <h1 className="text-4xl font-extrabold tracking-tight text-foreground sm:text-5xl lg:text-6xl">
                {t('about.hero.title')}
              </h1>
              <p className="text-xl text-muted-foreground sm:text-2xl">
                {t('about.hero.subtitle')}
              </p>
            </div>
          </div>
        </section>

        <section className="py-20 sm:py-32">
          <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
            <div className="grid grid-cols-1 items-center gap-16 lg:grid-cols-2">
              <div className="space-y-6 text-lg text-muted-foreground">
                <h2 className="text-3xl font-bold tracking-tight text-foreground sm:text-4xl">
                  {t('about.story.title')}
                </h2>
                <p>{t('about.story.p1')}</p>
                <p>{t('about.story.p2')}</p>
              </div>

              <div className="relative flex aspect-square items-center justify-center rounded-3xl border border-border bg-secondary/50 p-8 shadow-inner lg:aspect-auto lg:h-[500px]">
                <div className="absolute inset-0 flex items-center justify-center opacity-10">
                  <Brain className="h-64 w-64 text-primary" />
                </div>
                <div className="relative z-10 flex flex-col items-center gap-4 text-center">
                  <Sparkles className="h-12 w-12 text-yellow-500" />
                  <h3 className="text-2xl font-bold text-foreground">
                    {t('about.story.graphicTitle')}
                  </h3>
                </div>
              </div>
            </div>
          </div>
        </section>

        <section className="border-t border-border bg-secondary/20 py-20 sm:py-32">
          <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
            <div className="mb-16 text-center">
              <h2 className="text-3xl font-bold tracking-tight text-foreground sm:text-4xl">
                {t('about.values.title')}
              </h2>
            </div>

            <div className="grid grid-cols-1 gap-8 sm:grid-cols-2 lg:grid-cols-3">
              <div className="rounded-2xl border border-border bg-card p-8 shadow-sm">
                <Heart className="mb-4 h-8 w-8 text-rose-500" />
                <h3 className="mb-2 text-xl font-bold text-foreground">
                  {t('about.values.v1.title')}
                </h3>
                <p className="text-muted-foreground">
                  {t('about.values.v1.desc')}
                </p>
              </div>

              <div className="rounded-2xl border border-border bg-card p-8 shadow-sm">
                <Users className="mb-4 h-8 w-8 text-blue-500" />
                <h3 className="mb-2 text-xl font-bold text-foreground">
                  {t('about.values.v2.title')}
                </h3>
                <p className="text-muted-foreground">
                  {t('about.values.v2.desc')}
                </p>
              </div>

              <div className="rounded-2xl border border-border bg-card p-8 shadow-sm">
                <Shield className="mb-4 h-8 w-8 text-green-500" />
                <h3 className="mb-2 text-xl font-bold text-foreground">
                  {t('about.values.v3.title')}
                </h3>
                <p className="text-muted-foreground">
                  {t('about.values.v3.desc')}
                </p>
              </div>
            </div>
          </div>
        </section>

        <section className="py-20 sm:py-32">
          <div className="mx-auto max-w-4xl px-4 text-center sm:px-6 lg:px-8">
            <h2 className="mb-6 text-3xl font-bold tracking-tight text-foreground sm:text-4xl">
              {t('about.cta.title')}
            </h2>
            <p className="mb-8 text-lg text-muted-foreground">
              {t('about.cta.subtitle')}
            </p>
            <Link to="/register">
              <Button size="lg" className="h-14 px-10 text-lg">
                {t('landing.hero.startBtn')}
              </Button>
            </Link>
          </div>
        </section>
      </div>
    </div>
  )
}
