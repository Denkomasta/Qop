import { createFileRoute } from '@tanstack/react-router'
import { TeacherSubjectsView } from './-/TeacherSubjectsView'

export const Route = createFileRoute('/app/_authenticated/teacher/subjects/')({
  component: TeacherSubjectsView,
})
