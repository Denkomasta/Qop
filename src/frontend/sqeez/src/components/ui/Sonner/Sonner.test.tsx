import { render } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { Toaster } from './Sonner'

describe('Toaster', () => {
  it('renders without requiring a next-themes provider', () => {
    expect(() => render(<Toaster />)).not.toThrow()
  })
})
