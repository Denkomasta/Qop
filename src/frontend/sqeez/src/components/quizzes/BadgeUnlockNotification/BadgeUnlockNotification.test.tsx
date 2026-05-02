import { act } from 'react'
import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import type { StudentBadgeBasicDto } from '@/api/generated/model'
import { BadgeUnlockNotification } from './BadgeUnlockNotification'

describe('BadgeUnlockNotification', () => {
  it('reveals badges after a short delay and supports dismissal', async () => {
    vi.useFakeTimers()
    const badges = [
      { badgeId: 1, name: 'Fast Learner' },
      { badgeId: 2, name: 'Perfect Score', iconUrl: '/perfect.png' },
    ] as StudentBadgeBasicDto[]

    render(
      <BadgeUnlockNotification
        badges={badges}
        achievementText="Achievement unlocked"
      />,
    )

    expect(screen.queryByText('Fast Learner')).not.toBeInTheDocument()

    await act(async () => {
      await vi.advanceTimersByTimeAsync(500)
    })

    expect(screen.getByText('Fast Learner')).toBeInTheDocument()
    expect(screen.getByAltText('Perfect Score').getAttribute('src')).toContain(
      '/perfect.png',
    )

    fireEvent.click(screen.getAllByRole('button')[0])

    expect(screen.queryByText('Fast Learner')).not.toBeInTheDocument()
    expect(screen.getByText('Perfect Score')).toBeInTheDocument()

    vi.useRealTimers()
  })
})
