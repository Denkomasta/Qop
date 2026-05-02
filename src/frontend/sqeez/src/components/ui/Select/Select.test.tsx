import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectLabel,
  SelectSeparator,
  SelectTrigger,
  SelectValue,
} from './Select'

describe('Select', () => {
  it('renders trigger and open content', () => {
    render(
      <Select open value="math">
        <SelectTrigger aria-label="Subject">
          <SelectValue placeholder="Pick subject" />
        </SelectTrigger>
        <SelectContent>
          <SelectGroup>
            <SelectLabel>Subjects</SelectLabel>
            <SelectItem value="math">Math</SelectItem>
            <SelectSeparator />
            <SelectItem value="science">Science</SelectItem>
          </SelectGroup>
        </SelectContent>
      </Select>,
    )

    expect(screen.getByText('Subjects')).toBeInTheDocument()
    expect(screen.getByRole('option', { name: 'Math' })).toHaveAttribute(
      'data-state',
      'checked',
    )
    expect(screen.getByRole('option', { name: 'Science' })).toBeInTheDocument()
  })
})
