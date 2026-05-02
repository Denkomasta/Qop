import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { StudentBadge } from './StudentBadge'

describe('StudentBadge', () => {
  it('renders earned badge details with a formatted date', () => {
    render(
      <StudentBadge
        name="Quiz Master"
        iconUrl="/badges/master.png"
        earnedAt="2026-05-02T10:00:00.000Z"
      />,
    )

    expect(screen.getByText('Quiz Master')).toBeInTheDocument()
    expect(screen.getByAltText('Quiz Master').getAttribute('src')).toContain(
      '/badges/master.png',
    )
    expect(
      screen.getByText(
        new Date('2026-05-02T10:00:00.000Z').toLocaleDateString(),
      ),
    ).toBeInTheDocument()
  })

  it('renders locked state text', () => {
    render(
      <StudentBadge name="Hidden" isEarned={false} lockedText="Keep going" />,
    )

    expect(screen.getByText('Keep going')).toBeInTheDocument()
  })

  it('calls admin actions without bubbling', () => {
    const onEdit = vi.fn()
    const onDelete = vi.fn()

    render(
      <StudentBadge
        name="Editable"
        isAdmin
        onEdit={onEdit}
        onDelete={onDelete}
      />,
    )

    fireEvent.click(screen.getByTitle('Edit Badge'))
    fireEvent.click(screen.getByTitle('Delete Badge'))

    expect(onEdit).toHaveBeenCalled()
    expect(onDelete).toHaveBeenCalled()
  })
})
