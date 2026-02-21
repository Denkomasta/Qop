import { Link } from '@tanstack/react-router'
import { Menu } from 'lucide-react'
import { Button } from '@/components/ui/Button'
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from '@/components/ui/Sheet'
import { LanguageSwitcher } from '@/components/settings/LanguageSwitcher/LanguageSwitcher'
import { ThemeSwitcher } from '@/components/settings/ThemeSwitcher/ThemeSwitcher'
import SqeezLogo from '@/components/icons/logos/SqeezLogo'

interface LinkProps {
  to: string
  label: string
}

interface NavbarProps {
  navLinks?: LinkProps[]
  title?: string
  loginButtonText?: string
  registerButtonText?: string
  navigationText?: string
}

export function Navbar({
  navLinks,
  title,
  loginButtonText,
  registerButtonText,
  navigationText,
}: NavbarProps) {
  return (
    <header className="sticky top-0 z-50 w-full border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
      <div className="container flex h-16 items-center justify-between">
        {/* Left: Logo & Desktop Nav */}
        <div className="flex items-center gap-8">
          <Link to="/" className="flex items-center gap-2 text-xl font-bold">
            <SqeezLogo size={64} backgroundColor="var(--background)" />
            <span>{title}</span>
          </Link>

          <nav className="hidden gap-6 md:flex">
            {navLinks?.map((link) => (
              <Link
                key={link.to}
                to={link.to}
                className="text-sm font-medium text-muted-foreground transition-colors hover:text-primary"
                activeProps={{ className: 'text-primary' }}
              >
                {link.label}
              </Link>
            ))}
          </nav>
        </div>

        {/* Right: Actions & Mobile Menu */}
        <div className="flex items-center gap-3">
          <div className="hidden gap-2 sm:flex">
            <Button variant="ghost" size="sm" asChild>
              <Link to="/login">{loginButtonText}</Link>
            </Button>
            <Button size="sm" asChild>
              <Link to="/register">{registerButtonText}</Link>
            </Button>
            <LanguageSwitcher />
            <ThemeSwitcher />
          </div>

          {/* Mobile Menu Trigger */}
          <Sheet>
            <SheetTrigger asChild>
              <Button variant="ghost" size="icon" className="md:hidden">
                <Menu className="h-5 w-5" />
              </Button>
            </SheetTrigger>
            <SheetContent side="left" className="w-75 sm:w-100">
              <SheetHeader>
                <SheetTitle className="text-left">{navigationText}</SheetTitle>
              </SheetHeader>
              <nav className="mt-8 flex flex-col gap-4">
                {navLinks?.map((link) => (
                  <Link
                    key={link.to}
                    to={link.to}
                    className="text-lg font-semibold text-muted-foreground"
                    activeProps={{ className: 'text-primary' }}
                  >
                    {link.label}
                  </Link>
                ))}
                <hr className="my-2" />
                <Button variant={'outline'} className="w-full" asChild>
                  <Link to="/login">{loginButtonText}</Link>
                </Button>
                <Button className="w-full" asChild>
                  <Link to="/register">{registerButtonText}</Link>
                </Button>
                <LanguageSwitcher />
                <ThemeSwitcher />
              </nav>
            </SheetContent>
          </Sheet>
        </div>
      </div>
    </header>
  )
}
