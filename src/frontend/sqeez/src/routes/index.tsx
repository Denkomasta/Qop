import { createFileRoute, Link } from '@tanstack/react-router'
import { Trophy, Zap, Target, ArrowRight, Sparkles } from 'lucide-react'
import { useTranslation } from 'react-i18next'

import { Button } from '@/components/ui/Button'

export const Route = createFileRoute('/')({
  component: Landing,
})

function Landing() {
  const { t } = useTranslation()

  return (
    <div className="flex min-h-screen flex-col bg-background text-foreground">
      <div className="flex-1">
        <section className="relative overflow-hidden py-20 sm:py-32 lg:pb-32 xl:pb-36">
          <div className="mx-auto max-w-7xl px-4 text-center sm:px-6 lg:px-8">
            <div className="mx-auto max-w-3xl space-y-8">
              <div className="inline-flex items-center rounded-full border border-border bg-secondary/50 px-3 py-1 text-sm font-medium">
                <Sparkles className="mr-2 h-4 w-4 text-yellow-500" />
                <span>{t('landing.hero.pill')}</span>
              </div>

              <h1 className="text-4xl font-extrabold tracking-tight text-foreground sm:text-5xl md:text-6xl lg:text-7xl">
                {t('landing.hero.title1')}{' '}
                <span className="text-primary">
                  {t('landing.hero.titleHighlight')}
                </span>
              </h1>

              <p className="mx-auto max-w-2xl text-lg text-muted-foreground sm:text-xl">
                {t('landing.hero.description')}
              </p>

              <div className="flex flex-col items-center justify-center gap-4 sm:flex-row">
                <Link to="/register">
                  <Button
                    size="lg"
                    className="h-12 w-full gap-2 px-8 text-base sm:w-auto"
                  >
                    {t('landing.hero.startBtn')}
                    <ArrowRight className="h-4 w-4" />
                  </Button>
                </Link>
                <Link to="/about">
                  <Button
                    size="lg"
                    variant="outline"
                    className="h-12 w-full px-8 text-base sm:w-auto"
                  >
                    {t('landing.hero.learnMoreBtn')}
                  </Button>
                </Link>
              </div>
            </div>
          </div>
        </section>

        <div className="border-t border-border bg-secondary/20 py-20 sm:py-32">
          <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
            <div className="mb-16 text-center">
              <h2 className="text-3xl font-bold tracking-tight text-foreground sm:text-4xl">
                {t('landing.features.title')}
              </h2>
              <p className="mt-4 text-lg text-muted-foreground">
                {t('landing.features.subtitle')}
              </p>
            </div>

            <div className="grid grid-cols-1 gap-8 md:grid-cols-3">
              <div className="flex flex-col items-center rounded-2xl border border-border bg-card p-8 text-center shadow-sm transition-shadow hover:shadow-md">
                <div className="mb-6 flex h-14 w-14 items-center justify-center rounded-full bg-primary/10">
                  <Zap className="h-7 w-7 text-primary" />
                </div>
                <h3 className="mb-3 text-xl font-bold text-foreground">
                  {t('landing.features.quizzes.title')}
                </h3>
                <p className="text-muted-foreground">
                  {t('landing.features.quizzes.desc')}
                </p>
              </div>

              <div className="flex flex-col items-center rounded-2xl border border-border bg-card p-8 text-center shadow-sm transition-shadow hover:shadow-md">
                <div className="mb-6 flex h-14 w-14 items-center justify-center rounded-full bg-yellow-500/10">
                  <Trophy className="h-7 w-7 text-yellow-500" />
                </div>
                <h3 className="mb-3 text-xl font-bold text-foreground">
                  {t('landing.features.badges.title')}
                </h3>
                <p className="text-muted-foreground">
                  {t('landing.features.badges.desc')}
                </p>
              </div>

              <div className="flex flex-col items-center rounded-2xl border border-border bg-card p-8 text-center shadow-sm transition-shadow hover:shadow-md">
                <div className="mb-6 flex h-14 w-14 items-center justify-center rounded-full bg-green-500/10">
                  <Target className="h-7 w-7 text-green-500" />
                </div>
                <h3 className="mb-3 text-xl font-bold text-foreground">
                  {t('landing.features.progress.title')}
                </h3>
                <p className="text-muted-foreground">
                  {t('landing.features.progress.desc')}
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
