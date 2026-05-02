import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { AsyncButton } from './AsyncButton'
import { Button } from './Button'

describe('Button', () => {
  it('renders as a button with variant and size data attributes', () => {
    render(
      <Button variant="outline" size="lg">
        Save
      </Button>,
    )

    const button = screen.getByRole('button', { name: 'Save' })
    expect(button).toHaveAttribute('data-variant', 'outline')
    expect(button).toHaveAttribute('data-size', 'lg')
  })

  it('supports asChild links', () => {
    render(
      <Button asChild>
        <a href="/dashboard">Dashboard</a>
      </Button>,
    )

    expect(screen.getByRole('link', { name: 'Dashboard' })).toHaveAttribute(
      'href',
      '/dashboard',
    )
  })
})

describe('AsyncButton', () => {
  it('shows loading text while an async click is pending', async () => {
    let resolveClick!: () => void
    const onClick = vi.fn(
      () =>
        new Promise<void>((resolve) => {
          resolveClick = resolve
        }),
    )

    render(
      <AsyncButton onClick={onClick} loadingText="Saving...">
        Save
      </AsyncButton>,
    )

    fireEvent.click(screen.getByRole('button', { name: 'Save' }))

    expect(await screen.findByRole('button', { name: /Saving/ })).toBeDisabled()

    resolveClick()

    await waitFor(() =>
      expect(screen.getByRole('button', { name: 'Save' })).not.toBeDisabled(),
    )
  })

  it('uses external loading state', () => {
    render(
      <AsyncButton isLoading loadingText="Loading">
        Submit
      </AsyncButton>,
    )

    expect(screen.getByRole('button', { name: /Loading/ })).toBeDisabled()
  })
})
