import { useTranslation } from 'react-i18next'
import { CheckCircle2, XCircle, ArrowRight, Clock, Timer } from 'lucide-react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card'
import { Button } from '@/components/ui/Button'
import type { DetailedQuizQuestionDto } from '@/api/generated/model'

interface QuestionRecapScreenProps {
  question: DetailedQuizQuestionDto
  selectedOptionIds: (number | string)[]
  correctOptionIds: (number | string)[]
  userFreeTextAnswer?: string
  correctFreeTextAnswer?: string | null
  timeSpentMs?: number | null
  onContinue: () => void
  isLastQuestion: boolean
}

export function QuestionRecapScreen({
  question,
  selectedOptionIds,
  correctOptionIds,
  userFreeTextAnswer = '',
  correctFreeTextAnswer,
  timeSpentMs,
  onContinue,
  isLastQuestion,
}: QuestionRecapScreenProps) {
  const { t } = useTranslation()

  const isFreeTextQuestion = question.options.some((o) => o.isFreeText)

  const isFullyCorrect =
    !isFreeTextQuestion &&
    selectedOptionIds.length > 0 &&
    selectedOptionIds.length === correctOptionIds.length &&
    selectedOptionIds.every((id) => correctOptionIds.includes(id))

  const formattedTime = timeSpentMs
    ? `${(timeSpentMs / 1000).toFixed(1)}s`
    : null

  let bannerClass = ''
  let TitleIcon = null
  let titleText = ''
  let descText = ''
  let titleColor = ''
  let descColor = ''

  if (isFreeTextQuestion) {
    bannerClass =
      'border-amber-200 bg-amber-50 dark:border-amber-900/50 dark:bg-amber-900/20'
    TitleIcon = <Clock className="h-8 w-8 text-amber-600 dark:text-amber-500" />
    titleText = t('quiz.pendingReviewTitle')
    descText = t('quiz.pendingReviewDesc')
    titleColor = 'text-amber-700 dark:text-amber-400'
    descColor = 'text-amber-700/80 dark:text-amber-500/80'
  } else if (isFullyCorrect) {
    bannerClass =
      'border-green-200 bg-green-50 dark:border-green-900/50 dark:bg-green-900/20'
    TitleIcon = (
      <CheckCircle2 className="h-8 w-8 text-green-600 dark:text-green-500" />
    )
    titleText = t('quiz.correctAnswer')
    descText = t('quiz.greatJob')
    titleColor = 'text-green-700 dark:text-green-400'
    descColor = 'text-green-600 dark:text-green-500/80'
  } else {
    bannerClass = 'border-destructive/20 bg-destructive/10'
    TitleIcon = <XCircle className="h-8 w-8 text-destructive" />
    titleText = t('quiz.incorrectAnswer')
    descText = t('quiz.reviewAnswer')
    titleColor = 'text-destructive'
    descColor = 'text-destructive/80'
  }

  return (
    <div className="mx-auto flex min-h-[calc(100vh-4rem)] max-w-3xl animate-in flex-col p-4 duration-500 fade-in slide-in-from-bottom-4 md:p-6 lg:p-8">
      <div
        className={`mb-6 flex items-center gap-3 rounded-xl border p-4 shadow-sm ${bannerClass}`}
      >
        {TitleIcon}
        <div>
          <h2 className={`text-xl font-bold ${titleColor}`}>{titleText}</h2>
          <p className={`text-sm ${descColor}`}>{descText}</p>
        </div>

        {formattedTime && (
          <div className="ml-auto flex items-center gap-1.5 rounded-full border border-border/50 bg-background/60 px-3 py-1 text-sm font-semibold shadow-sm">
            <Timer className="h-4 w-4 opacity-70" />
            <span>{formattedTime}</span>
          </div>
        )}
      </div>

      <Card className="mb-8 flex-1 border-primary/10 shadow-md">
        <CardHeader className="border-b bg-muted/20 pb-6">
          <CardTitle className="text-xl leading-relaxed text-foreground md:text-2xl">
            {question.title}
          </CardTitle>
        </CardHeader>

        <CardContent className="pt-6">
          {isFreeTextQuestion ? (
            <div className="space-y-6">
              <div className="space-y-2">
                <p className="text-sm font-medium text-muted-foreground">
                  {t('quiz.yourAnswer')}
                </p>
                <div className="rounded-md border border-border bg-card p-4 text-base text-foreground shadow-sm">
                  {userFreeTextAnswer ? (
                    <span className="whitespace-pre-wrap">
                      {userFreeTextAnswer}
                    </span>
                  ) : (
                    <span className="italic opacity-70">
                      {t('quiz.noAnswerProvided')}
                    </span>
                  )}
                </div>
              </div>

              {correctFreeTextAnswer && (
                <div className="space-y-2">
                  <p className="flex items-end justify-between text-sm font-medium text-muted-foreground">
                    {t('quiz.expectedAnswerLabel')}
                    <span className="text-xs font-normal italic opacity-70">
                      {t('quiz.gradingGuideNote')}
                    </span>
                  </p>
                  <div className="rounded-md border border-muted-foreground/20 bg-muted/30 p-4 text-base text-muted-foreground">
                    <span className="whitespace-pre-wrap">
                      {correctFreeTextAnswer}
                    </span>
                  </div>
                </div>
              )}
            </div>
          ) : (
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
          )}
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
