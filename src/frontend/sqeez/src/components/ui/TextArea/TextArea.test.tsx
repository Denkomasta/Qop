import { act } from 'react'
import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { DebouncedTextArea } from './DebouncedTextArea'
import { TextArea } from './TextArea'

describe('TextArea', () => {
  it('links label and textarea by id', () => {
    render(<TextArea id="bio" label="Biography" />)

    expect(screen.getByLabelText('Biography')).toHaveAttribute('id', 'bio')
  })

  it('renders error text unless errors are hidden', () => {
    const { rerender } = render(<TextArea error="Required" />)

    expect(screen.getByText('Required')).toBeInTheDocument()

    rerender(<TextArea error="Required" hideErrors />)

    expect(screen.queryByText('Required')).not.toBeInTheDocument()
  })
})

describe('DebouncedTextArea', () => {
  it('saves changed text after the debounce delay', async () => {
    vi.useFakeTimers()
    const onSave = vi.fn().mockResolvedValue(undefined)

    render(
      <DebouncedTextArea
        initialValue="Initial"
        label="Question"
        onSave={onSave}
      />,
    )

    fireEvent.change(screen.getByRole('textbox'), {
      target: { value: 'Updated' },
    })

    expect(screen.getByText('Saving...')).toBeInTheDocument()

    await act(async () => {
      await vi.advanceTimersByTimeAsync(1000)
    })

    expect(onSave).toHaveBeenCalledWith('Updated')
    expect(screen.getByText('Saved')).toBeInTheDocument()

    vi.useRealTimers()
  })

  it('shows an error status when saving fails', async () => {
    vi.useFakeTimers()
    const onSave = vi.fn().mockRejectedValue(new Error('Nope'))

    render(
      <DebouncedTextArea
        initialValue="Initial"
        label="Question"
        onSave={onSave}
        errorText="Could not save"
      />,
    )

    fireEvent.change(screen.getByRole('textbox'), {
      target: { value: 'Broken' },
    })

    await act(async () => {
      await vi.advanceTimersByTimeAsync(1000)
    })

    expect(screen.getByText('Could not save')).toBeInTheDocument()

    vi.useRealTimers()
  })
})
