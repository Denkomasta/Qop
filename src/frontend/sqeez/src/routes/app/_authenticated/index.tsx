import { createFileRoute, Link } from '@tanstack/react-router'
import { useAuthStore } from '@/store/useAuthStore'
import {
  Card,
  CardHeader,
  CardTitle,
  CardDescription,
} from '@/components/ui/Card'
import {
  BookOpen,
  GraduationCap,
  Trophy,
  FileSignature,
  Users,
  Settings,
  ShieldAlert,
  Library,
} from 'lucide-react'
import { useTranslation } from 'react-i18next'

export const Route = createFileRoute('/app/_authenticated/')({
  component: DashboardLaunchpad,
})

type NavCard = {
  title: string
  description: string
  icon: React.ReactNode
  href: string
  colorClass: string
}

// 1. Move NavCardGrid HERE (outside the main component)
const NavCardGrid = ({ items }: { items: NavCard[] }) => (
  <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
    {items.map((item) => (
      <Link
        key={item.href}
        to={item.href}
        className="block rounded-xl ring-primary outline-none focus-visible:ring-2"
      >
        <Card className="group h-full cursor-pointer transition-all hover:border-primary hover:shadow-md">
          <CardHeader>
            <div
              className={`mb-4 inline-flex rounded-lg p-3 transition-transform group-hover:scale-110 ${item.colorClass}`}
            >
              {item.icon}
            </div>
            <CardTitle className="text-xl">{item.title}</CardTitle>
            <CardDescription className="line-clamp-2">
              {item.description}
            </CardDescription>
          </CardHeader>
        </Card>
      </Link>
    ))}
  </div>
)

// 2. Main Component
function DashboardLaunchpad() {
  const { t } = useTranslation()
  const user = useAuthStore((s) => s.user)

  if (!user) return null

  const isTeacher = user.role === 'Teacher' || user.role === 'Admin'
  const isAdmin = user.role === 'Admin'

  const studentLinks: NavCard[] = [
    {
      title: t('dashboard.subjects'),
      description: t('dashboard.subjectDescription'),
      icon: <Library className="h-8 w-8" />,
      href: '/app/subjects',
      colorClass: 'bg-blue-500/10 text-blue-500',
    },
    {
      title: t('dashboard.myClass'),
      description: t('dashboard.myClassDescripiton'),
      icon: <Users className="h-8 w-8" />,
      href: '/app/class',
      colorClass: 'bg-green-500/10 text-green-500',
    },
    {
      title: t('dashboard.quizzes'),
      description: t('dashboard.quizzesDescription'),
      icon: <FileSignature className="h-8 w-8" />,
      href: '/app/quizzes',
      colorClass: 'bg-purple-500/10 text-purple-500',
    },
    {
      title: t('dashboard.leaderboards'),
      description: t('dashboard.leaderboardsDescription'),
      icon: <Trophy className="h-8 w-8" />,
      href: '/app/leaderboard',
      colorClass: 'bg-yellow-500/10 text-yellow-500',
    },
  ]

  const teacherLinks: NavCard[] = [
    {
      title: t('dashboard.manageQuizzes'),
      description: t('dashboard.manageQuizzesDescription'),
      icon: <GraduationCap className="h-8 w-8" />,
      href: '/app/teacher/quizzes',
      colorClass: 'bg-orange-500/10 text-orange-500',
    },
    {
      title: t('dashboard.classManagement'),
      description: t('dashboard.classManagementDescription'),
      icon: <BookOpen className="h-8 w-8" />,
      href: '/app/teacher/classes',
      colorClass: 'bg-teal-500/10 text-teal-500',
    },
  ]

  const adminLinks: NavCard[] = [
    {
      title: t('dashboard.userManagement'),
      description: t('dashboard.userManagementDescription'),
      icon: <ShieldAlert className="h-8 w-8" />,
      href: '/app/admin/users',
      colorClass: 'bg-destructive/10 text-destructive',
    },
    {
      title: t('dashboard.systemSettings'),
      description: t('dashboard.systemSettingsDescription'),
      icon: <Settings className="h-8 w-8" />,
      href: '/app/admin/settings',
      colorClass: 'bg-zinc-500/10 text-zinc-500',
    },
  ]

  return (
    <div className="container mx-auto max-w-7xl space-y-10 p-6">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">
          {t('common.welcome')}, {user.username}!
        </h1>
        <p className="mt-1 text-muted-foreground">
          {t('dashboard.navigationDecription')}
        </p>
      </div>

      <section className="space-y-4">
        <h2 className="text-xl font-semibold tracking-tight">
          {t('dashboard.yourLearning')}
        </h2>
        <NavCardGrid items={studentLinks} />
      </section>

      {isTeacher && (
        <section className="space-y-4">
          <h2 className="text-xl font-semibold tracking-tight">
            {t('dashboard.teachingTools')}
          </h2>
          <NavCardGrid items={teacherLinks} />
        </section>
      )}

      {isAdmin && (
        <section className="space-y-4">
          <h2 className="text-xl font-semibold tracking-tight text-destructive">
            {t('dashboard.administration')}
          </h2>
          <NavCardGrid items={adminLinks} />
        </section>
      )}
    </div>
  )
}
