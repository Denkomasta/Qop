import type { DetailedQuizQuestionDto } from '@/api/generated/model'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card'
import { QuizOptionItem } from '../QuizOptionItem'

interface QuestionCardProps {
  question: DetailedQuizQuestionDto
  selectedOptionIds: (number | string)[]
  onSelectOption: (
    questionId: number | string,
    optionId: number | string,
  ) => void
  renderMediaAsset?: (
    assetId: number | string,
    isOption?: boolean,
  ) => React.ReactNode
  freeTextValue?: string
  onChangeFreeText?: (questionId: number | string, text: string) => void
}

export function QuestionCard({
  question,
  selectedOptionIds,
  onSelectOption,
  renderMediaAsset,
  freeTextValue = '',
  onChangeFreeText,
}: QuestionCardProps) {
  return (
    <Card className="flex-1 border-primary/10 shadow-md">
      <CardHeader className="border-b bg-muted/20 pb-6">
        <CardTitle className="text-xl leading-relaxed md:text-2xl">
          {question.title}
        </CardTitle>

        {question.mediaAssetId && renderMediaAsset && (
          <div className="mt-4 overflow-hidden rounded-xl border bg-background/50">
            {renderMediaAsset(question.mediaAssetId, false)}
          </div>
        )}
      </CardHeader>

      <CardContent className="pt-6">
        <div className="space-y-4">
          {question.options.map((option) => (
            <QuizOptionItem
              key={option.id}
              option={option}
              isSelected={selectedOptionIds.includes(option.id)}
              onSelect={() => onSelectOption(question.id, option.id)}
              freeTextValue={freeTextValue}
              onFreeTextChange={(text) => onChangeFreeText?.(question.id, text)}
              mediaNode={
                option.mediaAssetId && renderMediaAsset
                  ? renderMediaAsset(option.mediaAssetId, true)
                  : undefined
              }
            />
          ))}
        </div>
      </CardContent>
    </Card>
  )
}
