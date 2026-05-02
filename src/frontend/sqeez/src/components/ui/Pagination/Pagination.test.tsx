import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { Pagination } from './Pagination'

describe('Pagination', () => {
  it('does not render for a single page', () => {
    const { container } = render(
      <Pagination currentPage={1} totalPages={1} onPageChange={vi.fn()} />,
    )

    expect(container).toBeEmptyDOMElement()
  })

  it('navigates to previous and next pages', () => {
    const onPageChange = vi.fn()

    render(
      <Pagination currentPage={2} totalPages={4} onPageChange={onPageChange} />,
    )

    fireEvent.click(screen.getByRole('button', { name: /common.previous/ }))
    fireEvent.click(screen.getByRole('button', { name: /common.next/ }))

    expect(onPageChange).toHaveBeenNthCalledWith(1, 1)
    expect(onPageChange).toHaveBeenNthCalledWith(2, 3)
  })

  it('disables edge navigation buttons', () => {
    render(<Pagination currentPage={1} totalPages={4} onPageChange={vi.fn()} />)

    expect(
      screen.getByRole('button', { name: /common.previous/ }),
    ).toBeDisabled()
  })
})
