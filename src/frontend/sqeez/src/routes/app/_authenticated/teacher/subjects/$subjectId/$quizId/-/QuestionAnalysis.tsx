import { useTranslation } from 'react-i18next'
import { CheckCircle2, XCircle } from 'lucide-react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card'

// Replace with your actual Orval DTOs
export interface OptionStat {
  id: string | number
  text: string
  pickCount: number
  isCorrect: boolean
}

export interface QuestionStat {
  id: string | number
  questionText: string
  totalAnswers: number
  options: OptionStat[]
}

interface QuestionAnalysisProps {
  questions: QuestionStat[]
}

export function QuestionAnalysis({ questions }: QuestionAnalysisProps) {
  const { t } = useTranslation()

  if (!questions || questions.length === 0) {
    return null
  }

  return (
    <div className="flex flex-col gap-6">
      <div className="mb-2">
        <h2 className="text-xl font-semibold tracking-tight">
          {t('quiz.questionAnalysis', 'Question Analysis')}
        </h2>
        <p className="text-sm text-muted-foreground">
          {t(
            'quiz.questionAnalysisDesc',
            'See how students answered each individual question.',
          )}
        </p>
      </div>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        {questions.map((question, index) => (
          <Card key={question.id} className="border-border shadow-sm">
            <CardHeader className="pb-4">
              <CardTitle className="text-base leading-relaxed">
                <span className="mr-2 text-muted-foreground">{index + 1}.</span>
                {question.questionText}
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="flex flex-col gap-4">
                {question.options.map((option) => {
                  const percentage =
                    question.totalAnswers > 0
                      ? Math.round(
                          (option.pickCount / question.totalAnswers) * 100,
                        )
                      : 0

                  return (
                    <div key={option.id} className="flex flex-col gap-1.5">
                      <div className="flex items-start justify-between gap-4 text-sm">
                        <div className="flex flex-1 items-start gap-2">
                          {option.isCorrect ? (
                            <CheckCircle2 className="mt-0.5 h-4 w-4 shrink-0 text-emerald-500" />
                          ) : (
                            <XCircle className="mt-0.5 h-4 w-4 shrink-0 text-muted-foreground/30" />
                          )}
                          <span
                            className={
                              option.isCorrect
                                ? 'font-medium text-foreground'
                                : 'text-muted-foreground'
                            }
                          >
                            {option.text}
                          </span>
                        </div>
                        <div className="flex items-center gap-2 text-right">
                          <span className="font-medium text-foreground">
                            {percentage}%
                          </span>
                          <span className="w-12 text-xs text-muted-foreground">
                            ({option.pickCount})
                          </span>
                        </div>
                      </div>

                      <div className="ml-6 h-2 overflow-hidden rounded-full bg-muted">
                        <div
                          className={`h-full rounded-full transition-all duration-1000 ease-out ${
                            option.isCorrect
                              ? 'bg-emerald-500'
                              : 'bg-primary/40'
                          }`}
                          style={{ width: `${percentage}%` }}
                        />
                      </div>
                    </div>
                  )
                })}
              </div>
              <div className="mt-6 border-t pt-4 text-xs text-muted-foreground">
                {t('quiz.totalResponses', {
                  count: question.totalAnswers,
                  defaultValue: `${question.totalAnswers} responses`,
                })}
              </div>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  )
}
