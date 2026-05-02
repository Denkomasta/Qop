import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import {
  Avatar,
  AvatarBadge,
  AvatarFallback,
  AvatarGroup,
  AvatarGroupCount,
  AvatarImage,
} from './Avatar'
import { SimpleAvatar } from './SimpleAvatar'

describe('Avatar primitives', () => {
  it('renders fallback, image, badge and group count', () => {
    render(
      <AvatarGroup>
        <Avatar size="lg">
          <AvatarImage src="/avatar.png" alt="User avatar" />
          <AvatarFallback>DU</AvatarFallback>
          <AvatarBadge data-testid="avatar-badge" />
        </Avatar>
        <AvatarGroupCount>+3</AvatarGroupCount>
      </AvatarGroup>,
    )

    expect(screen.getByText('DU')).toBeInTheDocument()
    expect(screen.getByText('+3')).toHaveAttribute(
      'data-slot',
      'avatar-group-count',
    )
    expect(screen.getByTestId('avatar-badge')).toHaveAttribute(
      'data-slot',
      'avatar-badge',
    )
  })
})

describe('SimpleAvatar', () => {
  it('uses initials when no image url is supplied', () => {
    render(<SimpleAvatar firstName="Dana" lastName="User" />)

    expect(screen.getByText('DU')).toBeInTheDocument()
  })

  it('falls back to username initials', () => {
    render(<SimpleAvatar username="student" />)

    expect(screen.getByText('ST')).toBeInTheDocument()
  })
})
