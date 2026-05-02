import { act } from 'react'
import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { ConfirmModal } from './ConfirmModal'
import { BaseModal } from './Modal'

describe('BaseModal', () => {
  it('renders title, description, children and footer when open', () => {
    render(
      <BaseModal
        isOpen
        onClose={vi.fn()}
        title="Edit item"
        description="Change details"
        footer={<button>Done</button>}
      >
        Form content
      </BaseModal>,
    )

    expect(screen.getByText('Edit item')).toBeInTheDocument()
    expect(screen.getByText('Change details')).toBeInTheDocument()
    expect(screen.getByText('Form content')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Done' })).toBeInTheDocument()
  })
})

describe('ConfirmModal', () => {
  it('calls close and confirm callbacks', async () => {
    const onClose = vi.fn()
    const onConfirm = vi.fn()

    render(
      <ConfirmModal
        isOpen
        onClose={onClose}
        onConfirm={onConfirm}
        title="Delete subject"
        description="This cannot be undone"
        confirmText="Delete"
        cancelText="Cancel"
        isDestructive
      />,
    )

    fireEvent.click(screen.getByRole('button', { name: 'Cancel' }))
    await act(async () => {
      fireEvent.click(screen.getByRole('button', { name: 'Delete' }))
    })

    expect(onClose).toHaveBeenCalled()
    expect(onConfirm).toHaveBeenCalled()
  })
})
