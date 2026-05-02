import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { DebouncedInput } from './DebouncedInput'

describe('DebouncedInput', () => {
  it('calls onChange after the debounce delay', () => {
    vi.useFakeTimers()
    const onChange = vi.fn()

    render(
      <DebouncedInput
        value="math"
        onChange={onChange}
        debounceTime={250}
        id="search"
        label="Search"
      />,
    )

    fireEvent.change(screen.getByLabelText('Search'), {
      target: { value: 'science' },
    })

    expect(onChange).not.toHaveBeenCalled()

    vi.advanceTimersByTime(250)

    expect(onChange).toHaveBeenCalledWith('science')
    vi.useRealTimers()
  })

  it('updates local value when the controlled value changes', () => {
    const { rerender } = render(
      <DebouncedInput value="first" onChange={vi.fn()} label="Name" />,
    )

    rerender(
      <DebouncedInput
        value="second"
        onChange={vi.fn()}
        id="name"
        label="Name"
      />,
    )

    expect(screen.getByLabelText('Name')).toHaveValue('second')
  })
})
