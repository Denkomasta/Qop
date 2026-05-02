import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { Label } from './Label'

describe('Label', () => {
  it('associates text with a form control', () => {
    render(
      <>
        <Label htmlFor="email">Email</Label>
        <input id="email" />
      </>,
    )

    expect(screen.getByLabelText('Email')).toHaveAttribute('id', 'email')
  })
})
