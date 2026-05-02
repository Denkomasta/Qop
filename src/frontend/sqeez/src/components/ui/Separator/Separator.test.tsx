import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { Separator } from './Separator'

describe('Separator', () => {
  it('renders a non-decorative separator when requested', () => {
    render(<Separator decorative={false} orientation="vertical" />)

    const separator = screen.getByRole('separator')
    expect(separator).toHaveAttribute('data-orientation', 'vertical')
  })
})
