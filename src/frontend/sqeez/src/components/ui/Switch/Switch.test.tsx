import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { Switch } from './Switch'

describe('Switch', () => {
  it('renders size metadata and reports checked changes', () => {
    const onCheckedChange = vi.fn()

    render(
      <Switch
        aria-label="Enable notifications"
        size="sm"
        onCheckedChange={onCheckedChange}
      />,
    )

    const switchControl = screen.getByRole('switch', {
      name: 'Enable notifications',
    })
    expect(switchControl).toHaveAttribute('data-size', 'sm')

    fireEvent.click(switchControl)

    expect(onCheckedChange).toHaveBeenCalledWith(true)
  })
})
