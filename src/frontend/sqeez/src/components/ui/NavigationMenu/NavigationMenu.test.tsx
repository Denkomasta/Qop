import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import {
  NavigationMenu,
  NavigationMenuItem,
  NavigationMenuLink,
  NavigationMenuList,
  NavigationMenuTrigger,
} from './NavigationMenu'

describe('NavigationMenu', () => {
  it('renders menu items, triggers and links', () => {
    render(
      <NavigationMenu viewport={false}>
        <NavigationMenuList>
          <NavigationMenuItem>
            <NavigationMenuTrigger>Courses</NavigationMenuTrigger>
          </NavigationMenuItem>
          <NavigationMenuItem>
            <NavigationMenuLink href="/courses">All courses</NavigationMenuLink>
          </NavigationMenuItem>
        </NavigationMenuList>
      </NavigationMenu>,
    )

    expect(screen.getByRole('button', { name: /Courses/ })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'All courses' })).toHaveAttribute(
      'href',
      '/courses',
    )
  })

  it('marks the root when viewport rendering is enabled', () => {
    const { container } = render(
      <NavigationMenu>
        <NavigationMenuList>
          <NavigationMenuItem>
            <NavigationMenuLink href="/courses">Courses</NavigationMenuLink>
          </NavigationMenuItem>
        </NavigationMenuList>
      </NavigationMenu>,
    )

    expect(
      container.querySelector('[data-slot="navigation-menu"]'),
    ).toHaveAttribute('data-viewport', 'true')
  })
})
