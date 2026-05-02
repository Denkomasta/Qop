import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import {
  Accordion,
  AccordionContent,
  AccordionItem,
  AccordionTrigger,
} from './Accordion'

describe('Accordion', () => {
  it('renders an opened item', () => {
    render(
      <Accordion type="single" defaultValue="item-1">
        <AccordionItem value="item-1">
          <AccordionTrigger>Question</AccordionTrigger>
          <AccordionContent>Answer</AccordionContent>
        </AccordionItem>
      </Accordion>,
    )

    expect(screen.getByRole('button', { name: 'Question' })).toHaveAttribute(
      'data-state',
      'open',
    )
    expect(screen.getByText('Answer')).toBeInTheDocument()
  })
})
