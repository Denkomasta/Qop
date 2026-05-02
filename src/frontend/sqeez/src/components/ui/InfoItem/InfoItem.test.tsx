import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { EditableInfoItem } from './EditableInfoItem'
import { InfoItem } from './InfoItem'

describe('InfoItem', () => {
  it('renders label, value, icon and action', () => {
    render(
      <InfoItem
        icon={<span data-testid="icon" />}
        label="Email"
        value="student@example.com"
        action={<button>Copy</button>}
      />,
    )

    expect(screen.getByTestId('icon')).toBeInTheDocument()
    expect(screen.getByText('Email')).toBeInTheDocument()
    expect(screen.getByText('student@example.com')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Copy' })).toBeInTheDocument()
  })
})

describe('EditableInfoItem', () => {
  it('calls onEdit with field metadata', () => {
    const onEdit = vi.fn()

    render(
      <EditableInfoItem
        icon={<span />}
        label="Username"
        value="dana"
        fieldKey="username"
        buttonText="Edit"
        onEdit={onEdit}
      />,
    )

    fireEvent.click(screen.getByRole('button', { name: /Edit Username/ }))

    expect(onEdit).toHaveBeenCalledWith('username', 'Username', 'dana')
  })

  it('hides edit action when editing is disabled', () => {
    render(
      <EditableInfoItem
        icon={<span />}
        label="Username"
        value="dana"
        fieldKey="username"
        canEdit={false}
        onEdit={vi.fn()}
      />,
    )

    expect(screen.queryByRole('button')).not.toBeInTheDocument()
  })
})
