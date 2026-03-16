import { createFileRoute } from '@tanstack/react-router'
import { BookOpen, GraduationCap, Users, Mail, School } from 'lucide-react'
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/Card'
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/Avatar'
import { Badge } from '@/components/ui/Badge/Badge'
import { ScrollArea } from '@/components/ui/ScrollArea'
import { useTranslation } from 'react-i18next'
import { useExtendedUserProfile } from '@/hooks/useExtendedUserProfile'
import { useAuthStore } from '@/store/useAuthStore'
import { useGetApiClassesId } from '@/api/generated/endpoints/school-classes/school-classes'
import { Spinner } from '@/components/ui/Spinner'

export const Route = createFileRoute('/app/_authenticated/class/')({
  component: ClassPage,
})

interface Teacher {
  id: number
  firstName: string
  lastName: string
  email: string
  avatarUrl?: string
}

interface Student {
  id: number
  firstName: string
  lastName: string
  email: string
  avatarUrl?: string
}

interface Subject {
  id: number
  name: string
  code: string
}

interface SchoolClass {
  id: number
  name: string
  academicYear: string
  section: string
  teacher?: Teacher
  students: Student[]
  subjects: Subject[]
}

// --- MOCK DATA (Remove when connecting to your API) ---
const mockClassData: SchoolClass = {
  id: 1,
  name: 'Computer Science 101',
  academicYear: '2025/2026',
  section: 'Room A-12',
  teacher: {
    id: 1,
    firstName: 'Alan',
    lastName: 'Turing',
    email: 'alan.turing@school.edu',
  },
  subjects: [
    { id: 1, name: 'Algorithms', code: 'CS-101' },
    { id: 2, name: 'Data Structures', code: 'CS-102' },
    { id: 3, name: 'Web Development', code: 'CS-103' },
  ],
  students: Array.from({ length: 15 }).map((_, i) => ({
    id: i,
    firstName: `Student`,
    lastName: `${i + 1}`,
    email: `student${i + 1}@school.edu`,
  })),
}

export function ClassPage() {
  const { t } = useTranslation()
  const user = useAuthStore((s) => s.user)
  const { data: userData, isLoading: userLoading } = useExtendedUserProfile(
    user?.id,
    user?.role,
  )

  const classId = userData?.schoolClassId

  const {
    data: classData,
    isLoading: classLoading,
    isError,
  } = useGetApiClassesId(classId!, {
    query: { enabled: !!classId },
  })

  if (userLoading || (classId && classLoading)) {
    return (
      <div className="flex min-h-[60vh] flex-col items-center justify-center gap-4">
        <Spinner size={'lg'} />
        <p className="animate-pulse font-medium text-muted-foreground">
          {t('common.loading')}...
        </p>
      </div>
    )
  }

  if (!classId || !classData) {
    return (
      <div className="container mx-auto flex min-h-[60vh] items-center justify-center p-6">
        <Card className="w-full max-w-md border-2 border-dashed text-center shadow-sm">
          <CardHeader>
            <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-secondary">
              <School className="h-8 w-8 text-muted-foreground" />
            </div>
            <CardTitle className="text-2xl">No Class Assigned</CardTitle>
            <CardDescription className="mt-2 text-base">
              It looks like you haven't been assigned to a class for the current
              academic year yet.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <p className="rounded-md bg-muted p-4 text-sm text-muted-foreground">
              Please contact your school administrator or homeroom teacher to be
              added to your class roster. Once assigned, your schedule and
              classmates will appear here.
            </p>
          </CardContent>
        </Card>
      </div>
    )
  }

  return (
    <div className="container mx-auto space-y-6 p-6">
      {/* PAGE HEADER */}
      <div className="flex flex-col gap-2 md:flex-row md:items-center md:justify-between">
        <div>
          <h1 className="flex items-center gap-3 text-3xl font-bold tracking-tight">
            <GraduationCap className="h-8 w-8 text-primary" />
            {classData.name}
          </h1>
          <p className="mt-1 text-muted-foreground">
            {classData.academicYear} • {classData.section}
          </p>
        </div>
      </div>

      {/* MAIN GRID LAYOUT */}
      <div className="grid grid-cols-1 gap-6 md:grid-cols-3">
        {/* LEFT COLUMN: Teacher & Info */}
        <div className="space-y-6 md:col-span-1">
          <Card>
            <CardHeader>
              <CardTitle>Class Teacher</CardTitle>
            </CardHeader>
            <CardContent>
              {classData.teacher ? (
                <div className="flex items-center space-x-4">
                  <Avatar className="h-12 w-12">
                    <AvatarImage src={classData.teacher.avatarUrl} />
                    <AvatarFallback>
                      {classData.teacher.firstName[0]}
                      {classData.teacher.lastName[0]}
                    </AvatarFallback>
                  </Avatar>
                  <div>
                    <p className="text-sm leading-none font-medium">
                      {classData.teacher.firstName} {classData.teacher.lastName}
                    </p>
                    <p className="mt-1 flex items-center gap-1 text-sm text-muted-foreground">
                      <Mail className="h-3 w-3" />
                      {classData.teacher.email}
                    </p>
                  </div>
                </div>
              ) : (
                <p className="text-sm text-muted-foreground">
                  No teacher assigned.
                </p>
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <BookOpen className="h-5 w-5" />
                Subjects ({classData.subjects.length})
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="flex flex-wrap gap-2">
                {classData.subjects.map((subject) => (
                  <Badge key={subject.id} variant="secondary">
                    {subject.name}
                  </Badge>
                ))}
              </div>
            </CardContent>
          </Card>
        </div>

        {/* RIGHT COLUMN: Classmates Roster */}
        <div className="md:col-span-2">
          <Card className="flex h-full flex-col">
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Users className="h-5 w-5" />
                Classmates
              </CardTitle>
              <CardDescription>
                {classData.students.length} students enrolled in this class.
              </CardDescription>
            </CardHeader>
            <CardContent className="flex-1 p-0">
              {/* ScrollArea ensures the page doesn't get infinitely long if there are 40 students */}
              <ScrollArea className="h-[400px] px-6 pb-6">
                <div className="space-y-4">
                  {classData.students.map((student) => (
                    <div
                      key={student.id}
                      className="flex items-center justify-between rounded-lg border bg-card p-3 transition-colors hover:bg-accent/50"
                    >
                      <div className="flex items-center gap-4">
                        <Avatar>
                          <AvatarImage src={student.avatarUrl} />
                          <AvatarFallback>
                            {student.firstName[0]}
                            {student.lastName[0]}
                          </AvatarFallback>
                        </Avatar>
                        <div>
                          <p className="text-sm leading-none font-medium">
                            {student.firstName} {student.lastName}
                          </p>
                          <p className="mt-1 text-sm text-muted-foreground">
                            {student.email}
                          </p>
                        </div>
                      </div>
                      <Badge variant="outline">Student</Badge>
                    </div>
                  ))}
                </div>
              </ScrollArea>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  )
}
