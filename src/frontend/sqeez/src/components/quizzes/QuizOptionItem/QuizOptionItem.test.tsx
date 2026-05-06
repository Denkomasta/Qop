import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import type { StudentQuizOptionDto } from '@/api/generated/model'
import { QuizOptionItem } from './QuizOptionItem'

describe('QuizOptionItem', () => {
  it('renders a selectable option', () => {
    const onSelect = vi.fn()
    const option = { id: 1, text: 'Paris' } as StudentQuizOptionDto

    render(
      <QuizOptionItem option={option} isSelected={false} onSelect={onSelect} />,
    )

    fireEvent.click(screen.getByRole('button', { name: /Paris/ }))

    expect(onSelect).toHaveBeenCalled()
  })

  it('renders media and selected state for multiple choice', () => {
    const option = { id: 1, text: 'Image option' } as StudentQuizOptionDto

    render(
      <QuizOptionItem
        option={option}
        isSelected
        isMultiChoice
        onSelect={vi.fn()}
        mediaNode={<img src="/option.png" alt="Option media" />}
      />,
    )

    expect(screen.getByAltText('Option media')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Image option/ })).toHaveClass(
      'translate-y-2',
    )
  })

  it('renders a free-text option and reports focus and changes', () => {
    const onSelect = vi.fn()
    const onFreeTextChange = vi.fn()
    const option = {
      id: 1,
      text: 'Explain your answer',
      isFreeText: true,
    } as StudentQuizOptionDto

    render(
      <QuizOptionItem
        option={option}
        isSelected={false}
        onSelect={onSelect}
        freeTextValue=""
        onFreeTextChange={onFreeTextChange}
      />,
    )

    const textArea = screen.getByPlaceholderText('quiz.typeAnswerHere')
    fireEvent.focus(textArea)
    fireEvent.change(textArea, { target: { value: 'Because...' } })

    expect(onSelect).toHaveBeenCalled()
    expect(onFreeTextChange).toHaveBeenCalledWith('Because...')
  })
})
