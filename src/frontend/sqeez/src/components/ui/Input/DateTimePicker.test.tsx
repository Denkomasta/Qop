import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { DateTimePicker } from './DateTimePicker'

describe('DateTimePicker', () => {
  it('renders an ISO value as a local datetime input value', () => {
    const isoValue = '2026-05-02T10:30:00.000Z'
    const expectedLocalValue = new Date(
      new Date(isoValue).getTime() -
        new Date(isoValue).getTimezoneOffset() * 60000,
    )
      .toISOString()
      .slice(0, 16)

    render(
      <DateTimePicker
        data-testid="starts-at"
        value={isoValue}
        onChange={vi.fn()}
      />,
    )

    expect(screen.getByTestId('starts-at')).toHaveValue(expectedLocalValue)
  })

  it('emits an ISO string on blur after editing', () => {
    const onChange = vi.fn()

    render(
      <DateTimePicker
        data-testid="starts-at"
        value={null}
        onChange={onChange}
      />,
    )

    const input = screen.getByTestId('starts-at')
    fireEvent.change(input, { target: { value: '2026-06-10T14:45' } })
    fireEvent.blur(input)

    expect(onChange).toHaveBeenCalledWith(
      new Date('2026-06-10T14:45').toISOString(),
    )
  })

  it('emits null when the input is cleared', () => {
    const onChange = vi.fn()

    render(
      <DateTimePicker
        data-testid="starts-at"
        value="2026-05-02T10:30:00.000Z"
        onChange={onChange}
      />,
    )

    const input = screen.getByTestId('starts-at')
    fireEvent.change(input, { target: { value: '' } })
    fireEvent.blur(input)

    expect(onChange).toHaveBeenCalledWith(null)
  })

  it('renders invalid incoming values as empty', () => {
    render(
      <DateTimePicker
        data-testid="starts-at"
        value="not-a-date"
        onChange={vi.fn()}
      />,
    )

    expect(screen.getByTestId('starts-at')).toHaveValue('')
  })
})
