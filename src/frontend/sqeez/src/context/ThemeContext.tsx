import { createContext, useContext, useEffect, useState } from 'react'
import { type Theme } from '@/types/theme'
import { DAISY_THEMES } from '@/constants/themes'

const ThemeContext = createContext<
  | {
      theme: Theme
      setTheme: (theme: Theme) => void
    }
  | undefined
>(undefined)

export function ThemeProvider({ children }: { children: React.ReactNode }) {
  const [theme, setTheme] = useState<Theme>(
    () => (localStorage.getItem('ui-theme') as Theme) || 'system',
  )

  useEffect(() => {
    const root = window.document.documentElement
    const allThemes = [...DAISY_THEMES]
    root.classList.remove(...allThemes)

    root.setAttribute('data-theme', theme)

    const darkThemes = [
      'dark',
      'synthwave',
      'halloween',
      'forest',
      'black',
      'luxury',
      'dracula',
      'business',
      'night',
      'coffee',
    ]
    if (darkThemes.includes(theme)) {
      root.classList.add('dark')
    }

    localStorage.setItem('ui-theme', theme)
  }, [theme])

  return (
    <ThemeContext.Provider value={{ theme, setTheme }}>
      {children}
    </ThemeContext.Provider>
  )
}

export const useTheme = () => {
  const context = useContext(ThemeContext)
  if (!context) throw new Error('useTheme must be used within a ThemeProvider')
  return context
}
