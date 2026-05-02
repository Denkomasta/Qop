import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { ThemeProvider } from '@/context/ThemeContext'
import { ThemeSwitcher } from './ThemeSwitcher'

describe('ThemeSwitcher', () => {
  it('opens a menu with theme choices', async () => {
    render(
      <ThemeProvider>
        <ThemeSwitcher title="Theme choices" />
      </ThemeProvider>,
    )

    fireEvent.pointerDown(screen.getByRole('button'))

    expect(await screen.findByText('Theme choices')).toBeInTheDocument()
    expect(screen.getByText('light')).toBeInTheDocument()
  })
})
