import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
} from './Sheet'

describe('Sheet', () => {
  it('renders open sheet content on the requested side', () => {
    render(
      <Sheet open>
        <SheetContent side="left">
          <SheetHeader>
            <SheetTitle>Navigation</SheetTitle>
            <SheetDescription>Main menu</SheetDescription>
          </SheetHeader>
        </SheetContent>
      </Sheet>,
    )

    expect(screen.getByRole('dialog')).toBeInTheDocument()
    expect(screen.getByText('Navigation')).toBeInTheDocument()
    expect(screen.getByText('Main menu')).toBeInTheDocument()
  })
})
