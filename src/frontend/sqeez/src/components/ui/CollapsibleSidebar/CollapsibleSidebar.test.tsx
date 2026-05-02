import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { CollapsibleSidebar } from './CollapsibleSidebar'

describe('CollapsibleSidebar', () => {
  it('collapses and expands around its children', () => {
    render(
      <CollapsibleSidebar
        title="Editor"
        collapseTooltip="Hide"
        expandTooltip="Show"
      >
        Sidebar content
      </CollapsibleSidebar>,
    )

    expect(screen.getByText('Editor')).toBeInTheDocument()
    expect(screen.getByText('Sidebar content')).toBeInTheDocument()

    fireEvent.click(screen.getByTitle('Hide'))

    expect(screen.queryByText('Sidebar content')).not.toBeInTheDocument()

    fireEvent.click(screen.getByTitle('Show'))

    expect(screen.getByText('Sidebar content')).toBeInTheDocument()
  })
})
