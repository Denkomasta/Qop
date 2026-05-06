import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import type { DetailedQuizQuestionDto } from '@/api/generated/model'
import { QuestionCard } from './QuestionCard'

const question = {
  id: 10,
  title: 'What is 2 + 2?',
  isStrictMultipleChoice: false,
  mediaAssetId: 99,
  options: [
    { id: 1, text: '3' },
    { id: 2, text: '4', mediaAssetId: 100 },
  ],
} as DetailedQuizQuestionDto

describe('QuestionCard', () => {
  it('renders the question, hint, media and options', () => {
    render(
      <QuestionCard
        question={question}
        selectedOptionIds={[2]}
        onSelectOption={vi.fn()}
        renderMediaAsset={(assetId, isOption) => (
          <span>{isOption ? `option-${assetId}` : `question-${assetId}`}</span>
        )}
      />,
    )

    expect(screen.getByText('What is 2 + 2?')).toBeInTheDocument()
    expect(screen.getByText('quiz.selectSingleHint')).toBeInTheDocument()
    expect(screen.getByText('question-99')).toBeInTheDocument()
    expect(screen.getByText('option-100')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /4/ })).toHaveClass(
      'translate-y-2',
    )
  })

  it('reports selected option ids', () => {
    const onSelectOption = vi.fn()

    render(
      <QuestionCard
        question={question}
        selectedOptionIds={[]}
        onSelectOption={onSelectOption}
      />,
    )

    fireEvent.click(screen.getByRole('button', { name: /3/ }))

    expect(onSelectOption).toHaveBeenCalledWith(1)
  })

  it('passes free-text changes through', () => {
    const onChangeFreeText = vi.fn()
    const freeTextQuestion = {
      ...question,
      options: [{ id: 3, text: 'Write it', isFreeText: true }],
    } as DetailedQuizQuestionDto

    render(
      <QuestionCard
        question={freeTextQuestion}
        selectedOptionIds={[]}
        onSelectOption={vi.fn()}
        onChangeFreeText={onChangeFreeText}
      />,
    )

    fireEvent.change(screen.getByPlaceholderText('quiz.typeAnswerHere'), {
      target: { value: 'Four' },
    })

    expect(onChangeFreeText).toHaveBeenCalledWith('Four')
  })
})
