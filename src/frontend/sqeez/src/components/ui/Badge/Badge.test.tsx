import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { Badge } from './Badge'

describe('Badge', () => {
  it('renders content with variant metadata', () => {
    render(<Badge variant="secondary">New</Badge>)

    expect(screen.getByText('New')).toHaveAttribute('data-variant', 'secondary')
  })

  it('can render as a child element', () => {
    render(
      <Badge asChild>
        <a href="/badges">Badge link</a>
      </Badge>,
    )

    expect(screen.getByRole('link', { name: 'Badge link' })).toHaveAttribute(
      'href',
      '/badges',
    )
  })
})
