import { createFileRoute } from '@tanstack/react-router'
import { useTranslation } from 'react-i18next'
import { Lock } from 'lucide-react'
import { useSystemConfig } from '@/hooks/useSystemConfig'
import { Spinner } from '@/components/ui/Spinner'

export const Route = createFileRoute('/privacy/')({
  component: PrivacyPolicy,
})

function PrivacyPolicy() {
  const { t } = useTranslation()
  const { config, isLoading: isSystemConfigLoading } = useSystemConfig()

  return (
    <div className="flex min-h-screen flex-col bg-background text-foreground">
      <section className="border-b border-border bg-secondary/20 py-16 sm:py-24">
        <div className="mx-auto max-w-4xl px-4 text-center sm:px-6 lg:px-8">
          <Lock className="mx-auto mb-6 h-12 w-12 text-primary" />
          <h1 className="text-3xl font-extrabold tracking-tight text-foreground sm:text-5xl">
            {t('privacy.title')}
          </h1>
          <p className="mt-4 text-lg text-muted-foreground">
            {t('privacy.lastUpdated', {
              date: '4. 4. 2026',
            })}
          </p>
        </div>
      </section>

      <section className="py-16 sm:py-24">
        <div className="mx-auto max-w-3xl px-4 sm:px-6 lg:px-8">
          <div className="space-y-12 text-base leading-7 text-muted-foreground">
            <div className="space-y-4">
              <h2 className="text-2xl font-bold tracking-tight text-foreground">
                1. {t('privacy.intro.title')}
              </h2>
              <p>{t('privacy.intro.p1')}</p>
            </div>

            <div className="space-y-4">
              <h2 className="text-2xl font-bold tracking-tight text-foreground">
                2. {t('privacy.collection.title')}
              </h2>
              <p>{t('privacy.collection.p1')}</p>
              <ul className="ml-6 list-disc space-y-2">
                <li>{t('privacy.collection.rule1')}</li>
                <li>{t('privacy.collection.rule2')}</li>
              </ul>
            </div>

            <div className="space-y-4">
              <h2 className="text-2xl font-bold tracking-tight text-foreground">
                3. {t('privacy.storage.title')}
              </h2>
              <p>{t('privacy.storage.p1')}</p>
            </div>

            <div className="space-y-4">
              <h2 className="text-2xl font-bold tracking-tight text-foreground">
                4. {t('privacy.rights.title')}
              </h2>
              <p>{t('privacy.rights.p1')}</p>
            </div>

            <div className="mt-12 rounded-xl border border-border bg-secondary/30 p-6 sm:p-8">
              <h3 className="text-xl font-bold text-foreground">
                {t('privacy.contact.title')}
              </h3>
              <p className="mt-2">
                {t('privacy.contact.desc')}
                {isSystemConfigLoading ? (
                  <Spinner size="lg" />
                ) : (
                  <a
                    href={`mailto:${config?.supportEmail}`}
                    className="font-semibold text-primary hover:underline"
                  >
                    {config?.supportEmail}
                  </a>
                )}
              </p>
            </div>
          </div>
        </div>
      </section>
    </div>
  )
}
