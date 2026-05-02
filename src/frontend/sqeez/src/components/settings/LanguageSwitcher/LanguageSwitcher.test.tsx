import { fireEvent, render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { LanguageSwitcher } from './LanguageSwitcher'

describe('LanguageSwitcher', () => {
  afterEach(() => {
    vi.restoreAllMocks()
  })

  beforeEach(() => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        json: () =>
          Promise.resolve([{ code: 'en', label: 'English', flag: 'EN' }]),
      }),
    )
  })

  it('loads and displays the current language', async () => {
    render(<LanguageSwitcher />)

    expect(await screen.findByText('English')).toBeInTheDocument()
    expect(screen.getByText('EN')).toBeInTheDocument()
  })

  it('renders language menu options after loading', async () => {
    render(<LanguageSwitcher />)

    expect(await screen.findByText('English')).toBeInTheDocument()

    fireEvent.pointerDown(screen.getByRole('button'))

    expect(
      await screen.findByRole('menuitem', { name: /English/ }),
    ).toBeInTheDocument()
  })

  it('logs a useful error when languages cannot be loaded', async () => {
    const consoleErrorSpy = vi
      .spyOn(console, 'error')
      .mockImplementation(() => undefined)

    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new Error('Network down')))

    render(<LanguageSwitcher />)

    await vi.waitFor(() =>
      expect(consoleErrorSpy).toHaveBeenCalledWith(
        'Could not load languages',
        expect.any(Error),
      ),
    )
  })
})
