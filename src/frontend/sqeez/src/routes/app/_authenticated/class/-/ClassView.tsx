import { Link } from '@tanstack/react-router'
import {
  BookOpen,
  GraduationCap,
  Users,
  Mail,
  School,
  Star,
} from 'lucide-react'
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/Card'
import { SimpleAvatar } from '@/components/ui/Avatar'
import { Badge } from '@/components/ui/Badge/Badge'
import { ScrollArea } from '@/components/ui/ScrollArea'
import { useTranslation } from 'react-i18next'
import { useExtendedUserProfile } from '@/hooks/useExtendedUserProfile'
import { useAuthStore } from '@/store/useAuthStore'
import { useGetApiClassesId } from '@/api/generated/endpoints/school-classes/school-classes'
import { Spinner } from '@/components/ui/Spinner'
import { calculateLevel, formatName } from '@/lib/userHelpers'

export function ClassView({ targetClassId }: { targetClassId?: number }) {
  const { t } = useTranslation()
  const user = useAuthStore((s) => s.user)

  const { data: userData, isLoading: userLoading } = useExtendedUserProfile(
    user?.id,
    user?.role,
    // { query: { enabled: !targetClassId } }, TODO use one EP.
  )

  const resolvedClassId = targetClassId || userData?.schoolClassId

  const { data: classData, isLoading: classLoading } = useGetApiClassesId(
    resolvedClassId ? Number(resolvedClassId) : 0,
    {
      query: { enabled: !!resolvedClassId },
    },
  )

  if ((!targetClassId && userLoading) || (resolvedClassId && classLoading)) {
    return (
      <div className="flex min-h-[60vh] flex-col items-center justify-center gap-4">
        <Spinner size={'lg'} />
        <p className="animate-pulse font-medium text-muted-foreground">
          {t('common.loading')}...
        </p>
      </div>
    )
  }

  if (!resolvedClassId || !classData) {
    return (
      <div className="container mx-auto flex min-h-[60vh] items-center justify-center p-6">
        <Card className="w-full max-w-md border-2 border-dashed text-center shadow-sm">
          <CardHeader>
            <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-secondary">
              <School className="h-8 w-8 text-muted-foreground" />
            </div>
            <CardTitle className="text-2xl">
              {t('class.noClassTitle')}
            </CardTitle>
            <CardDescription className="mt-2 text-base">
              {t('class.noClassDesc')}
            </CardDescription>
          </CardHeader>
          <CardContent>
            <p className="rounded-md bg-muted p-4 text-sm text-muted-foreground">
              {t('class.noClassAction')}
            </p>
          </CardContent>
        </Card>
      </div>
    )
  }

  return (
    <div className="container mx-auto space-y-6 p-6">
      <div className="flex flex-col gap-2 md:flex-row md:items-center md:justify-between">
        <div>
          <h1 className="flex items-center gap-3 text-3xl font-bold tracking-tight">
            <GraduationCap className="h-8 w-8 text-primary" />
            {classData.name}
            {userData?.schoolClassId === classData.id && (
              <Badge
                variant="outline"
                className="ml-2 border-primary/20 bg-primary/10 align-middle text-primary"
              >
                {t('class.myClass', 'My Class')}
              </Badge>
            )}
          </h1>
          <p className="mt-1 text-muted-foreground">
            {classData.academicYear} • {classData.section}
          </p>
        </div>
      </div>

      <div className="grid grid-cols-1 gap-6 md:grid-cols-3">
        <div className="space-y-6 md:col-span-1">
          <Card>
            <CardHeader>
              <CardTitle>{t('class.teacherTitle')}</CardTitle>
            </CardHeader>
            <CardContent>
              {classData.teacher ? (
                <div className="flex items-center space-x-4">
                  <Link
                    to="/app/profile/$userId"
                    params={{ userId: (classData.teacher.id ?? 0).toString() }}
                    className="transition-opacity hover:opacity-80"
                  >
                    <SimpleAvatar
                      url={classData.teacher.avatarUrl}
                      firstName={classData.teacher.firstName}
                      lastName={classData.teacher.lastName}
                      wrapperClassName="size-12"
                    />
                  </Link>
                  <div>
                    <p className="text-sm leading-none font-medium">
                      <Link
                        to="/app/profile/$userId"
                        params={{
                          userId: (classData.teacher.id ?? 0).toString(),
                        }}
                        className="hover:underline"
                      >
                        {formatName(
                          classData.teacher.firstName ?? 'John',
                          classData.teacher.lastName ?? 'Doe',
                        )}
                      </Link>
                    </p>
                    <p className="mt-1 flex items-center gap-1 text-sm text-muted-foreground">
                      <Mail className="h-3 w-3" />
                      {classData.teacher.email}
                    </p>
                  </div>
                </div>
              ) : (
                <p className="text-sm text-muted-foreground">
                  {t('class.noTeacher')}
                </p>
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <BookOpen className="h-5 w-5" />
                {t('class.subjects', {
                  count: classData.subjects?.length ?? 0,
                })}
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="flex flex-wrap gap-2">
                {classData.subjects?.map((subject) => (
                  <Link
                    key={subject.id}
                    to="/app/subject/$subjectId"
                    params={{ subjectId: (subject.id ?? 0).toString() }}
                  >
                    <Badge
                      variant="secondary"
                      className="cursor-pointer transition-colors hover:bg-secondary/80"
                    >
                      {subject.name}
                    </Badge>
                  </Link>
                ))}
              </div>
            </CardContent>
          </Card>
        </div>

        <div className="md:col-span-2">
          <Card className="flex h-full flex-col">
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Users className="h-5 w-5" />
                {t('class.classmatesTitle')}
              </CardTitle>
              <CardDescription>
                {t('class.studentsCount', {
                  count: classData.students?.length ?? 0,
                })}
              </CardDescription>
            </CardHeader>
            <CardContent className="flex-1 p-0">
              <ScrollArea className="h-100 px-6 pb-6">
                <div className="space-y-4">
                  {classData.students?.map((student) => {
                    const isMe = student.id === user?.id
                    const level = calculateLevel(student.currentXp)

                    return (
                      <div
                        key={student.id}
                        className={`flex items-center justify-between rounded-lg border p-3 transition-colors ${
                          isMe
                            ? 'border-primary/50 bg-primary/10'
                            : 'bg-card hover:bg-accent/50'
                        }`}
                      >
                        <div className="flex items-center gap-4">
                          <Link
                            to="/app/profile/$userId"
                            params={{ userId: (student.id ?? 0).toString() }}
                            className="transition-opacity hover:opacity-80"
                          >
                            <SimpleAvatar
                              url={student.avatarUrl}
                              firstName={student.firstName}
                              lastName={student.lastName}
                            />
                          </Link>
                          <div>
                            <p className="flex items-center gap-2 text-sm leading-none font-medium">
                              <Link
                                to="/app/profile/$userId"
                                params={{
                                  userId: (student.id ?? 0).toString(),
                                }}
                                className="hover:underline"
                              >
                                {formatName(
                                  student.firstName,
                                  student.lastName ?? 'Doe',
                                )}
                              </Link>
                              {isMe && (
                                <Badge
                                  variant="default"
                                  className="h-5 px-1.5 text-[10px]"
                                >
                                  {t('class.me')}
                                </Badge>
                              )}
                            </p>
                            <p className="mt-1 flex items-center gap-1 text-sm text-muted-foreground">
                              {student.email}
                            </p>
                          </div>
                        </div>

                        <div className="flex items-center gap-2">
                          <Badge
                            variant="outline"
                            className="flex items-center gap-1"
                          >
                            <Star className="h-3 w-3 fill-yellow-500 text-yellow-500" />
                            {t('class.level', { level })}
                          </Badge>
                        </div>
                      </div>
                    )
                  })}
                </div>
              </ScrollArea>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  )
}
