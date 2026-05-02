import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { Checkbox } from './Checkbox'

describe('Checkbox', () => {
  it('renders as an accessible checkbox and reports checked changes', () => {
    const onCheckedChange = vi.fn()

    render(
      <Checkbox aria-label="Accept terms" onCheckedChange={onCheckedChange} />,
    )

    const checkbox = screen.getByRole('checkbox', { name: 'Accept terms' })
    expect(checkbox).toHaveAttribute('data-slot', 'checkbox')

    fireEvent.click(checkbox)

    expect(onCheckedChange).toHaveBeenCalledWith(true)
  })
})
