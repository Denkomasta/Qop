import * as React from 'react'
import { render, screen } from '@testing-library/react'
import { describe, it, expect } from 'vitest'
import { Input } from './Input'

describe('Input Component', () => {
  it('renders a basic input element', () => {
    render(<Input placeholder="Enter your name" />)
    const input = screen.getByPlaceholderText('Enter your name')
    expect(input).toBeInTheDocument()
  })

  it('renders a label and links it to the input via id', () => {
    render(<Input id="email-input" label="Email Address" />)
    const input = screen.getByLabelText('Email Address')

    expect(input).toBeInTheDocument()
    expect(input).toHaveAttribute('id', 'email-input')
  })

  it('displays an error message and sets aria-invalid', () => {
    render(<Input error="This field is required" />)

    // Check if error text renders
    expect(screen.getByText('This field is required')).toBeInTheDocument()

    // Check if accessibility attribute updates correctly
    const input = screen.getByRole('textbox')
    expect(input).toHaveAttribute('aria-invalid', 'true')
  })

  it('does not set aria-invalid to true when there is no error', () => {
    render(<Input />)
    const input = screen.getByRole('textbox')
    expect(input).toHaveAttribute('aria-invalid', 'false')
  })

  it('renders an icon when provided', () => {
    render(<Input icon={<svg data-testid="search-icon" />} />)
    expect(screen.getByTestId('search-icon')).toBeInTheDocument()
  })

  it('renders the rightTopChip content when provided', () => {
    render(<Input rightTopChip={<span data-testid="chip">Optional</span>} />)
    expect(screen.getByTestId('chip')).toBeInTheDocument()
  })

  it('applies custom class names properly', () => {
    const { container } = render(
      <Input
        className="custom-input-class"
        containerClassName="custom-container-class"
      />,
    )

    const input = screen.getByRole('textbox')
    expect(input).toHaveClass('custom-input-class')

    // The outermost div should have the containerClassName
    expect(container.firstChild).toHaveClass('custom-container-class')
  })

  it('passes standard input attributes down to the input element', () => {
    render(
      <Input
        type="password"
        disabled
        required
        name="password"
        data-testid="attribute-test-input"
      />,
    )

    const input = screen.getByTestId('attribute-test-input')

    expect(input).toBeInTheDocument()
    expect(input).toHaveAttribute('type', 'password')
    expect(input).toBeDisabled()
    expect(input).toBeRequired()
  })

  it('correctly attaches a ref to the input element', () => {
    const ref = React.createRef<HTMLInputElement>()
    render(<Input ref={ref} />)

    expect(ref.current).not.toBeNull()
    expect(ref.current?.tagName).toBe('INPUT')
  })
})
