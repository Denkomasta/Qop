import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { BrandingPanel } from './BrandingPanel'

describe('BrandingPanel', () => {
  it('renders translated branding copy and achievement stats', () => {
    render(<BrandingPanel />)

    expect(screen.getByText('system.name')).toBeInTheDocument()
    expect(screen.getByText('brandingPanel.title')).toBeInTheDocument()
    expect(screen.getByText('12-day')).toBeInTheDocument()
    expect(screen.getByText('Speed Demon')).toBeInTheDocument()
    expect(screen.getByText(/24,000\+/)).toBeInTheDocument()
  })
})
