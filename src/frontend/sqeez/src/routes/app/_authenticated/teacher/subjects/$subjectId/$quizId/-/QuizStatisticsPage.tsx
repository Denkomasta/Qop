import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from '@tanstack/react-router'
import {
  ArrowLeft,
  FileSignature,
  Users,
  Target,
  Award,
  Search,
  Settings,
} from 'lucide-react'

import { PageLayout } from '@/components/layouting/PageLayout/PageLayout'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card'
import { Badge } from '@/components/ui/Badge/Badge'
import { Button } from '@/components/ui/Button'
import { DebouncedInput } from '@/components/ui/Input/DebouncedInput'
import { Pagination } from '@/components/ui/Pagination'

import { QuizAttemptsTable } from './QuizAttemptsTable'
import { QuestionAnalysis, type QuestionStat } from './QuestionAnalysis'

// ==========================================
// 🛑 MOCK DATA (Remove when API is ready)
// ==========================================
const MOCK_QUIZ_DATA = {
  title: 'Midterm Examination: React Fundamentals',
  publishDate: '2024-03-15T10:00:00Z',
  averageScore: 76,
  passRate: 85,
}

const MOCK_QUESTIONS: QuestionStat[] = [
  {
    id: 1,
    questionText: 'What is the primary purpose of React?',
    totalAnswers: 4,
    options: [
      {
        id: 'a',
        text: 'Building user interfaces',
        pickCount: 3,
        isCorrect: true,
      },
      { id: 'b', text: 'Managing databases', pickCount: 0, isCorrect: false },
      { id: 'c', text: 'Server-side routing', pickCount: 1, isCorrect: false },
      { id: 'd', text: 'Styling web pages', pickCount: 0, isCorrect: false },
    ],
  },
  {
    id: 2,
    questionText:
      'Which hook is used to manage local state in a functional component?',
    totalAnswers: 4,
    options: [
      { id: 'a', text: 'useEffect', pickCount: 1, isCorrect: false },
      { id: 'b', text: 'useState', pickCount: 2, isCorrect: true },
      { id: 'c', text: 'useContext', pickCount: 0, isCorrect: false },
      { id: 'd', text: 'useReducer', pickCount: 1, isCorrect: false },
    ],
  },
]

const MOCK_ATTEMPTS = [
  {
    id: 1,
    studentAvatarUrl: 'https://i.pravatar.cc/150?u=1',
    studentFirstName: 'Alice',
    studentLastName: 'Smith',
    studentEmail: 'alice.smith@school.edu',
    studentUsername: 'asmith',
    submittedAt: '2024-04-20T14:30:00Z',
    isGraded: true,
    score: 92,
    passMark: 50,
  },
  {
    id: 2,
    studentAvatarUrl: null,
    studentFirstName: 'Bob',
    studentLastName: 'Johnson',
    studentEmail: 'bob.j@school.edu',
    studentUsername: 'bjohnson',
    submittedAt: '2024-04-21T09:15:00Z',
    isGraded: true,
    score: 45, // Failed
    passMark: 50,
  },
  {
    id: 3,
    studentAvatarUrl: 'https://i.pravatar.cc/150?u=3',
    studentFirstName: 'Charlie',
    studentLastName: 'Brown',
    studentEmail: 'charlie.b@school.edu',
    studentUsername: 'cbrown',
    submittedAt: null, // In Progress
    isGraded: false,
    score: 0,
    passMark: 50,
  },
  {
    id: 4,
    studentAvatarUrl: null,
    studentFirstName: 'Diana',
    studentLastName: 'Prince',
    studentEmail: 'dprince@school.edu',
    studentUsername: 'dprince',
    submittedAt: '2024-04-21T11:45:00Z',
    isGraded: false, // Pending Grade
    score: 88,
    passMark: 50,
  },
]
// ==========================================

export function QuizStatisticsPage({
  subjectId,
  quizId,
}: {
  subjectId: string
  quizId: string
}) {
  const { t } = useTranslation()
  const [searchQuery, setSearchQuery] = useState('')
  const [pageNumber, setPageNumber] = useState(1)
  const pageSize = 15

  // 1. Fetch Quiz Data (MOCKED)
  // const { data: quizData, isLoading: isLoadingQuiz } = useGetApiQuizzesId(Number(quizId))
  const quizData = MOCK_QUIZ_DATA
  const isLoadingQuiz = false

  // 2. Fetch Attempts Data (MOCKED)
  // const { data: attemptsResponse, isLoading: isLoadingAttempts, isFetching } = useGetApiQuizzesIdAttempts(...)

  // Filter mock attempts based on search query just to make the UI feel alive
  const filteredAttempts = MOCK_ATTEMPTS.filter(
    (a) =>
      a.studentFirstName.toLowerCase().includes(searchQuery.toLowerCase()) ||
      a.studentLastName.toLowerCase().includes(searchQuery.toLowerCase()),
  )

  const attemptsResponse = {
    data: filteredAttempts,
    totalCount: filteredAttempts.length,
    totalPages: 1,
  }
  const isLoadingAttempts = false
  const isFetching = false

  const questionStats = MOCK_QUESTIONS

  const attempts = attemptsResponse?.data || []
  const totalAttempts = Number(attemptsResponse?.totalCount || 0)
  const totalPages = Number(attemptsResponse?.totalPages || 1)

  const isLoading = isLoadingQuiz || (isLoadingAttempts && !attemptsResponse)

  // STATS Mapping
  const averageScore = quizData?.averageScore || 0
  const passRate = quizData?.passRate || 0

  return (
    <PageLayout
      variant="app"
      containerClassName="max-w-7xl"
      isLoading={isLoading}
      title={
        <div className="flex flex-col items-start gap-4">
          <Link
            to="/app/teacher/quizzes"
            className="flex w-fit items-center gap-2 text-sm font-normal text-muted-foreground transition-colors hover:text-foreground"
          >
            <ArrowLeft className="h-4 w-4" />
            {t('quiz.backToQuizzes')}
          </Link>
          <div className="flex items-center gap-3">
            <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-purple-500/10 text-purple-500">
              <FileSignature className="h-6 w-6" />
            </div>
            <span>{quizData?.title}</span>
          </div>
        </div>
      }
      titleBadge={
        <Badge
          variant={quizData?.publishDate ? 'default' : 'secondary'}
          className="mt-8 shadow-sm"
        >
          {quizData?.publishDate ? t('quiz.published') : t('quiz.draft')}
        </Badge>
      }
      headerActions={
        <Link to="/app/quizzes/$quizId/builder" params={{ quizId }}>
          <Button variant="outline" className="mt-8 gap-2 shadow-sm">
            <Settings className="h-4 w-4" />
            {t('quiz.editQuiz')}
          </Button>
        </Link>
      }
      headerControls={
        <div className="flex w-full flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <DebouncedInput
            id="attempts-search"
            value={searchQuery}
            onChange={(val) => {
              setSearchQuery(val)
              setPageNumber(1)
            }}
            placeholder={t('quiz.searchStudents')}
            icon={<Search className="h-4 w-4" />}
            className="w-full bg-background sm:max-w-xs"
            hideErrors
          />
          <div className="text-sm font-medium whitespace-nowrap text-muted-foreground">
            {t('common.totalCount', {
              count: totalAttempts,
              defaultValue: `Total: ${totalAttempts}`,
            })}
          </div>
        </div>
      }
    >
      <div className="flex flex-col gap-8">
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
          <Card className="border-border shadow-sm transition-shadow hover:shadow-md">
            <CardHeader className="flex flex-row items-center justify-between pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">
                {t('quiz.totalAttempts')}
              </CardTitle>
              <Users className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold">{totalAttempts}</div>
            </CardContent>
          </Card>

          <Card className="border-border shadow-sm transition-shadow hover:shadow-md">
            <CardHeader className="flex flex-row items-center justify-between pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">
                {t('quiz.averageScore')}
              </CardTitle>
              <Target className="h-4 w-4 text-blue-500" />
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold text-blue-600">
                {averageScore}%
              </div>
            </CardContent>
          </Card>

          <Card className="border-border shadow-sm transition-shadow hover:shadow-md">
            <CardHeader className="flex flex-row items-center justify-between pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">
                {t('quiz.passRate')}
              </CardTitle>
              <Award className="h-4 w-4 text-emerald-500" />
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold text-emerald-600">
                {passRate}%
              </div>
            </CardContent>
          </Card>
        </div>

        <QuestionAnalysis questions={questionStats} />

        <div
          className={`transition-opacity duration-200 ${isFetching && !isLoadingAttempts ? 'opacity-50' : 'opacity-100'}`}
        >
          <div className="mb-4">
            <h2 className="text-xl font-semibold tracking-tight">
              {t('quiz.attemptHistory')}
            </h2>
          </div>

          <QuizAttemptsTable
            attempts={attempts}
            isLoading={isFetching || isLoadingAttempts}
          />

          {totalPages > 1 && (
            <div className="mt-8 flex justify-center">
              <Pagination
                currentPage={pageNumber}
                totalPages={totalPages}
                onPageChange={setPageNumber}
              />
            </div>
          )}
        </div>
      </div>
    </PageLayout>
  )
}
