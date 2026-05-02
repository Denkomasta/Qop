import { act } from 'react'
import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { ScrollableSelectList } from './ScrollableSelectList'

describe('ScrollableSelectList', () => {
  it('renders loading and empty states', () => {
    const { rerender } = render(
      <ScrollableSelectList
        options={[]}
        selectedId=""
        onSelect={vi.fn()}
        isLoading
        loadingText="Loading classes"
      />,
    )

    expect(screen.getByText('Loading classes')).toBeInTheDocument()

    rerender(
      <ScrollableSelectList
        options={[]}
        selectedId=""
        onSelect={vi.fn()}
        emptyText="No classes"
      />,
    )

    expect(screen.getByText('No classes')).toBeInTheDocument()
  })

  it('selects options and renders a load-more action', async () => {
    const onSelect = vi.fn()
    const onLoadMore = vi.fn()

    render(
      <ScrollableSelectList
        options={[
          { id: 1, title: 'Class A', subtitle: 'Teacher A' },
          { id: 2, title: 'Class B' },
        ]}
        selectedId={2}
        onSelect={onSelect}
        hasMore
        loadMoreText="More"
        onLoadMore={onLoadMore}
      />,
    )

    fireEvent.click(screen.getByText('Class A').closest('button')!)
    await act(async () => {
      fireEvent.click(screen.getByRole('button', { name: 'More' }))
    })

    expect(onSelect).toHaveBeenCalledWith(1)
    expect(onLoadMore).toHaveBeenCalled()
  })
})
