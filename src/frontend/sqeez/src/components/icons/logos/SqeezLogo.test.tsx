import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import SqeezLogo from './SqeezLogo'

describe('SqeezLogo', () => {
  it('renders an accessible svg with configured size and background', () => {
    render(<SqeezLogo size={48} backgroundColor="#ffffff" className="logo" />)

    const logo = screen.getByLabelText('Sqeez Logo')
    expect(logo).toHaveAttribute('width', '48')
    expect(logo).toHaveAttribute('height', '48')
    expect(logo).toHaveClass('logo')
    expect(logo).toHaveStyle({ backgroundColor: '#ffffff' })
  })
})
