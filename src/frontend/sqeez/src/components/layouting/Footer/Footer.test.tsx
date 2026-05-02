import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { Footer } from './Footer'

describe('Footer', () => {
  it('renders links and rights text', () => {
    render(
      <Footer
        rightsText="All rights reserved"
        links={[
          { to: '/privacy', label: 'Privacy' },
          { to: '/terms', label: 'Terms' },
        ]}
      />,
    )

    expect(screen.getByRole('link', { name: 'Privacy' })).toHaveAttribute(
      'href',
      '/privacy',
    )
    expect(screen.getByRole('link', { name: 'Terms' })).toHaveAttribute(
      'href',
      '/terms',
    )
    expect(screen.getByText('All rights reserved')).toBeInTheDocument()
  })
})
