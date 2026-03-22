import { useTranslation } from 'react-i18next'
import { CheckCircle2, XCircle, ArrowRight } from 'lucide-react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card'
import { Button } from '@/components/ui/Button'
import type { DetailedQuizQuestionDto } from '@/api/generated/model'

interface QuestionRecapScreenProps {
  question: DetailedQuizQuestionDto
  selectedOptionIds: (number | string)[]
  correctOptionIds: (number | string)[]
  onContinue: () => void
  isLastQuestion: boolean
}

export function QuestionRecapScreen({
  question,
  selectedOptionIds,
  correctOptionIds,
  onContinue,
  isLastQuestion,
}: QuestionRecapScreenProps) {
  const { t } = useTranslation()

  const isFullyCorrect =
    selectedOptionIds.length === correctOptionIds.length &&
    selectedOptionIds.every((id) => correctOptionIds.includes(id))

  return (
    <div className="mx-auto flex min-h-[calc(100vh-4rem)] max-w-3xl animate-in flex-col p-4 duration-500 fade-in slide-in-from-bottom-4 md:p-6 lg:p-8">
      <div
        className={`mb-6 flex items-center gap-3 rounded-xl border p-4 shadow-sm ${
          isFullyCorrect
            ? 'border-green-200 bg-green-50 dark:border-green-900/50 dark:bg-green-900/20'
            : 'border-destructive/20 bg-destructive/10'
        }`}
      >
        {isFullyCorrect ? (
          <CheckCircle2 className="h-8 w-8 text-green-600 dark:text-green-500" />
        ) : (
          <XCircle className="h-8 w-8 text-destructive" />
        )}
        <div>
          <h2
            className={`text-xl font-bold ${
              isFullyCorrect
                ? 'text-green-700 dark:text-green-400'
                : 'text-destructive'
            }`}
          >
            {isFullyCorrect
              ? t('quiz.correctAnswer')
              : t('quiz.incorrectAnswer')}
          </h2>
          <p
            className={`text-sm ${
              isFullyCorrect
                ? 'text-green-600 dark:text-green-500/80'
                : 'text-destructive/80'
            }`}
          >
            {isFullyCorrect ? t('quiz.greatJob') : t('quiz.reviewAnswer')}
          </p>
        </div>
      </div>

      <Card className="mb-8 flex-1 border-primary/10 shadow-md">
        <CardHeader className="border-b bg-muted/20 pb-6">
          <CardTitle className="text-xl leading-relaxed text-muted-foreground md:text-2xl">
            {question.title}
          </CardTitle>
        </CardHeader>

        <CardContent className="pt-6">
          <div className="space-y-3">
            {question.options.map((option) => {
              const isSelected = selectedOptionIds.includes(option.id)
              const isCorrect = correctOptionIds.includes(option.id)

              let optionStyle = 'border-border bg-card opacity-50'
              let Icon = null

              if (isSelected && isCorrect) {
                optionStyle =
                  'border-green-500 bg-green-50 dark:bg-green-900/20 ring-1 ring-green-500'
                Icon = (
                  <CheckCircle2 className="h-5 w-5 text-green-600 dark:text-green-400" />
                )
              } else if (isSelected && !isCorrect) {
                optionStyle =
                  'border-destructive bg-destructive/10 ring-1 ring-destructive'
                Icon = <XCircle className="h-5 w-5 text-destructive" />
              } else if (!isSelected && isCorrect) {
                optionStyle =
                  'border-green-500 border-dashed bg-green-50/50 dark:bg-green-900/10'
                Icon = (
                  <CheckCircle2 className="h-5 w-5 text-green-600/50 dark:text-green-400/50" />
                )
              }

              return (
                <div
                  key={option.id}
                  className={`flex w-full items-center gap-4 rounded-xl border p-4 text-left transition-all ${optionStyle}`}
                >
                  <div className="flex h-6 w-6 shrink-0 items-center justify-center">
                    {Icon ? (
                      Icon
                    ) : (
                      <div className="h-2 w-2 rounded-full bg-muted-foreground/30" />
                    )}
                  </div>

                  <span
                    className={`text-base font-medium ${
                      isSelected || isCorrect
                        ? 'text-foreground'
                        : 'text-muted-foreground'
                    }`}
                  >
                    {option.text}
                  </span>
                </div>
              )
            })}
          </div>
        </CardContent>
      </Card>

      <div className="mt-auto flex justify-end">
        <Button
          size="lg"
          onClick={onContinue}
          className="w-full shadow-md sm:w-auto"
        >
          {isLastQuestion ? t('quiz.finishQuiz') : t('common.continue')}
          <ArrowRight className="ml-2 h-5 w-5" />
        </Button>
      </div>
    </div>
  )
}
