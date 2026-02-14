import { Check, Palette } from 'lucide-react'
import { useTheme } from '@/context/ThemeContext'
import { Button } from '@/components/ui/Button'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/DropdownMenu'
import { ScrollArea } from '@/components/ui/ScrollArea'
import { DAISY_THEMES } from '@/constants/themes'

export function ThemeSwitcher() {
  const { theme: currentTheme, setTheme } = useTheme()

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button variant="ghost" size="icon">
          <Palette className="h-5 w-5" />
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="w-56">
        <DropdownMenuLabel>Themes</DropdownMenuLabel>
        <DropdownMenuSeparator />
        <ScrollArea className="h-72">
          {DAISY_THEMES.map((themeName) => (
            <DropdownMenuItem
              key={themeName}
              onClick={() => setTheme(themeName)}
              className="flex items-center justify-between capitalize"
            >
              {themeName}
              {currentTheme === themeName && (
                <Check className="h-4 w-4 opacity-50" />
              )}
            </DropdownMenuItem>
          ))}
        </ScrollArea>
      </DropdownMenuContent>
    </DropdownMenu>
  )
}
