import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { TabsWithMore } from './TabsWithMore'

describe('TabsWithMore', () => {
  it('renders visible links and active button tabs', () => {
    const onClick = vi.fn()

    render(
      <TabsWithMore
        tabs={[
          { id: 'home', label: 'Home', to: '/' },
          { id: 'custom', label: 'Custom', onClick, isActive: true },
        ]}
      />,
    )

    fireEvent.click(screen.getByRole('button', { name: 'Custom' }))

    expect(screen.getByRole('link', { name: 'Home' })).toHaveAttribute(
      'href',
      '/',
    )
    expect(screen.getByRole('button', { name: 'Custom' })).toHaveClass(
      'text-primary',
    )
    expect(onClick).toHaveBeenCalled()
  })

  it('moves extra tabs into the more menu', async () => {
    const onHiddenClick = vi.fn()

    render(
      <TabsWithMore
        maxVisible={1}
        tabs={[
          { id: 'first', label: 'First', to: '/first' },
          { id: 'second', label: 'Second', to: '/second' },
          { id: 'third', label: 'Third', onClick: onHiddenClick },
        ]}
      />,
    )

    expect(screen.getByRole('link', { name: 'First' })).toBeInTheDocument()
    fireEvent.pointerDown(screen.getByRole('button', { name: /common.more/ }))

    expect(
      await screen.findByRole('menuitem', { name: 'Second' }),
    ).toHaveAttribute('href', '/second')

    fireEvent.click(screen.getByRole('menuitem', { name: 'Third' }))

    expect(onHiddenClick).toHaveBeenCalled()
  })
})
