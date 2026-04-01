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
  School,
  BookCopy,
  Award,
  FileUp,
} from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { useGetApiUsersId } from '@/api/generated/endpoints/user/user'
import { Spinner } from '@/components/ui/Spinner'
import type { StudentDtoTeacherDto } from '@/api/generated/model'

export const Route = createFileRoute('/app/_authenticated/')({
  component: DashboardLaunchpad,
})

type NavCard = {
  title: string
  description: string
  icon: React.ReactNode
  href: string
  params?: Record<string, string | number>
  colorClass: string
}

const NavCardGrid = ({ items }: { items: NavCard[] }) => (
  <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
    {items.map((item) => (
      <Link
        key={item.href}
        to={item.href}
        params={item.params}
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

function DashboardLaunchpad() {
  const { t } = useTranslation()
  const user = useAuthStore((s) => s.user)

  const userId = user?.id
  const isTeacher = user?.role === 'Teacher' || user?.role === 'Admin'
  const isAdmin = user?.role === 'Admin'

  const { data, isLoading } = useGetApiUsersId(Number(userId), {
    query: { enabled: !!userId && isTeacher },
  })

  if (!user) return null

  if (isLoading) {
    return (
      <div className="flex min-h-[60vh] flex-col items-center justify-center gap-4">
        <Spinner size="lg" />
      </div>
    )
  }

  const teacherData = data as StudentDtoTeacherDto | undefined

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
      href: '/app/leaderboards',
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
    ...(teacherData?.managedClassId
      ? [
          {
            title: t('dashboard.classManagement'),
            description: t('dashboard.classManagementDescription'),
            icon: <BookOpen className="h-8 w-8" />,
            href: '/app/class/$classId',
            params: { classId: String(teacherData.managedClassId) },
            colorClass: 'bg-teal-500/10 text-teal-500',
          },
        ]
      : []),
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
      title: t('dashboard.adminClasses'),
      description: t('dashboard.adminClassesDescription'),
      icon: <School className="h-8 w-8" />,
      href: '/app/admin/classes',
      colorClass: 'bg-indigo-500/10 text-indigo-500',
    },
    {
      title: t('dashboard.adminSubjects'),
      description: t('dashboard.adminSubjectsDescription'),
      icon: <BookCopy className="h-8 w-8" />,
      href: '/app/admin/subjects',
      colorClass: 'bg-cyan-500/10 text-cyan-500',
    },
    {
      title: t('dashboard.adminBadges'),
      description: t('dashboard.adminBadgesDescription'),
      icon: <Award className="h-8 w-8" />,
      href: '/app/admin/badges',
      colorClass: 'bg-amber-500/10 text-amber-500',
    },
    {
      title: t('dashboard.adminImport'),
      description: t('dashboard.adminImportDescription'),
      icon: <FileUp className="h-8 w-8" />,
      href: '/app/admin/imports',
      colorClass: 'bg-emerald-500/10 text-emerald-500',
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
          <h2 className="text-xl font-semibold tracking-tight">
            {t('dashboard.administration')}
          </h2>
          <NavCardGrid items={adminLinks} />
        </section>
      )}
    </div>
  )
}
