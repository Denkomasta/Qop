import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import {
  DropdownMenu,
  DropdownMenuCheckboxItem,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuRadioGroup,
  DropdownMenuRadioItem,
  DropdownMenuSeparator,
  DropdownMenuShortcut,
  DropdownMenuSub,
  DropdownMenuSubContent,
  DropdownMenuSubTrigger,
  DropdownMenuTrigger,
} from './DropdownMenu'

describe('DropdownMenu', () => {
  it('renders open menu content and item variants', () => {
    render(
      <DropdownMenu open>
        <DropdownMenuTrigger>Open menu</DropdownMenuTrigger>
        <DropdownMenuContent>
          <DropdownMenuLabel>Actions</DropdownMenuLabel>
          <DropdownMenuItem>Profile</DropdownMenuItem>
          <DropdownMenuCheckboxItem checked>Enabled</DropdownMenuCheckboxItem>
          <DropdownMenuRadioGroup value="one">
            <DropdownMenuRadioItem value="one">One</DropdownMenuRadioItem>
          </DropdownMenuRadioGroup>
          <DropdownMenuItem variant="destructive">
            Delete
            <DropdownMenuShortcut>Del</DropdownMenuShortcut>
          </DropdownMenuItem>
          <DropdownMenuSeparator />
        </DropdownMenuContent>
      </DropdownMenu>,
    )

    expect(screen.getByText('Actions')).toBeInTheDocument()
    expect(
      screen.getByRole('menuitem', { name: 'Profile' }),
    ).toBeInTheDocument()
    expect(
      screen.getByRole('menuitemcheckbox', { name: 'Enabled' }),
    ).toHaveAttribute('data-state', 'checked')
    expect(screen.getByRole('menuitemradio', { name: 'One' })).toHaveAttribute(
      'data-state',
      'checked',
    )
    expect(screen.getByText('Del')).toHaveAttribute(
      'data-slot',
      'dropdown-menu-shortcut',
    )
  })

  it('renders submenu triggers and content', () => {
    render(
      <DropdownMenu open>
        <DropdownMenuTrigger>Open menu</DropdownMenuTrigger>
        <DropdownMenuContent>
          <DropdownMenuSub open>
            <DropdownMenuSubTrigger>More actions</DropdownMenuSubTrigger>
            <DropdownMenuSubContent>
              <DropdownMenuItem>Archive</DropdownMenuItem>
            </DropdownMenuSubContent>
          </DropdownMenuSub>
        </DropdownMenuContent>
      </DropdownMenu>,
    )

    expect(
      screen.getByRole('menuitem', { name: /More actions/ }),
    ).toBeInTheDocument()
    expect(
      screen.getByRole('menuitem', { name: 'Archive' }),
    ).toBeInTheDocument()
  })
})
