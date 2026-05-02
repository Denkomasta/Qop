import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import {
  Popover,
  PopoverContent,
  PopoverDescription,
  PopoverHeader,
  PopoverTitle,
  PopoverTrigger,
} from './Popover'

describe('Popover', () => {
  it('renders open popover content', () => {
    render(
      <Popover open>
        <PopoverTrigger>Open filters</PopoverTrigger>
        <PopoverContent>
          <PopoverHeader>
            <PopoverTitle>Filters</PopoverTitle>
            <PopoverDescription>Choose values</PopoverDescription>
          </PopoverHeader>
        </PopoverContent>
      </Popover>,
    )

    expect(screen.getByText('Filters')).toBeInTheDocument()
    expect(screen.getByText('Choose values')).toBeInTheDocument()
  })
})
