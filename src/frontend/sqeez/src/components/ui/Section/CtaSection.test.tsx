import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { CtaSection } from './CtaSection'

describe('CtaSection', () => {
  it('renders title, subtitle and action', () => {
    render(
      <CtaSection
        title="Ready?"
        subtitle="Start practicing"
        actionButton={<button>Begin</button>}
      />,
    )

    expect(screen.getByRole('heading', { name: 'Ready?' })).toBeInTheDocument()
    expect(screen.getByText('Start practicing')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Begin' })).toBeInTheDocument()
  })
})
