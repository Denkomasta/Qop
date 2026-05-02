import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { Spinner } from './Spinner'

describe('Spinner', () => {
  it('passes svg props and size classes through', () => {
    render(<Spinner aria-label="Loading" size="lg" />)

    expect(screen.getByLabelText('Loading')).toHaveClass('h-8', 'w-8')
  })
})
