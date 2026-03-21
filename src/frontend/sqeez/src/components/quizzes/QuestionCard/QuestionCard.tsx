import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card'

// Adjust these interfaces to match your exact generated Orval DTOs
export interface QuizOptionDto {
  id: number | string
  text: string
}

export interface QuizQuestionDto {
  id: number | string
  text: string
  options: QuizOptionDto[]
}

interface QuestionCardProps {
  question: QuizQuestionDto
  selectedOptionIds: (number | string)[]
  onSelectOption: (
    questionId: number | string,
    optionId: number | string,
  ) => void
}

export function QuestionCard({
  question,
  selectedOptionIds,
  onSelectOption,
}: QuestionCardProps) {
  return (
    <Card className="flex-1 border-primary/10 shadow-md">
      <CardHeader className="border-b bg-muted/20 pb-6">
        <CardTitle className="text-xl leading-relaxed md:text-2xl">
          {question.text}
        </CardTitle>
      </CardHeader>

      <CardContent className="pt-6">
        <div className="space-y-3">
          {question.options.map((option) => {
            const isSelected = selectedOptionIds.includes(option.id)

            return (
              <button
                key={option.id}
                onClick={() => onSelectOption(question.id, option.id)}
                className={`flex w-full items-center gap-4 rounded-xl border p-4 text-left transition-all focus:ring-2 focus:ring-primary focus:ring-offset-2 focus:outline-none ${
                  isSelected
                    ? 'border-primary bg-primary/5 shadow-sm ring-1 ring-primary'
                    : 'border-border bg-card hover:border-primary/50 hover:bg-muted/50'
                }`}
              >
                <div
                  className={`flex size-5 shrink-0 items-center justify-center rounded-full border ${
                    isSelected
                      ? 'border-primary bg-primary'
                      : 'border-muted-foreground'
                  }`}
                >
                  {isSelected && (
                    <div className="size-2 rounded-full bg-primary-foreground" />
                  )}
                </div>

                <span
                  className={`text-base ${
                    isSelected
                      ? 'font-medium text-foreground'
                      : 'text-muted-foreground'
                  }`}
                >
                  {option.text}
                </span>
              </button>
            )
          })}
        </div>
      </CardContent>
    </Card>
  )
}
