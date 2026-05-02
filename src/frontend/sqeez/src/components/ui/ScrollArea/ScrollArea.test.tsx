import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { ScrollArea } from './ScrollArea'

describe('ScrollArea', () => {
  it('renders children inside the viewport', () => {
    render(<ScrollArea>Scrollable content</ScrollArea>)

    expect(screen.getByText('Scrollable content')).toBeInTheDocument()
  })

  it('passes class names to the scroll area root', () => {
    const { container } = render(
      <ScrollArea className="custom-scroll-area">Content</ScrollArea>,
    )

    expect(container.querySelector('[data-slot="scroll-area"]')).toHaveClass(
      'custom-scroll-area',
    )
  })
})
