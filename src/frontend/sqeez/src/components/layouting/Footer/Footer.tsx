import { Link } from '@tanstack/react-router'
import { useTranslation } from 'react-i18next'

export function Footer() {
  const { t } = useTranslation()
  const currentYear = new Date().getFullYear()

  return (
    <footer className="w-full border-t bg-background">
      <div className="flex flex-col items-center justify-center gap-4 py-4 text-sm text-muted-foreground">
        <div className="flex flex-row gap-4">
          <Link to="/" className="transition-colors hover:text-foreground">
            {t('footer.help')}
          </Link>
          <Link to="/" className="transition-colors hover:text-foreground">
            {t('footer.privacy')}
          </Link>
          <Link to="/" className="transition-colors hover:text-foreground">
            {t('footer.terms')}
          </Link>
        </div>

        <p className="text-center text-sm leading-loose text-muted-foreground md:text-left">
          &copy; {currentYear} {t('system.name')}. {t('footer.rights')}
        </p>
      </div>
    </footer>
  )
}
