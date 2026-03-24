import { createFileRoute } from '@tanstack/react-router'
import { TeacherQuizzesPage } from './-/TeacherQuizzesPage'

export const Route = createFileRoute('/app/_authenticated/teacher/quizzes/')({
  component: TeacherQuizzesPage,
})
