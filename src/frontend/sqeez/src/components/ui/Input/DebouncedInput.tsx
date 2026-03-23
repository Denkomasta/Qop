import { useState, useEffect } from 'react'
import { Input } from '@/components/ui/Input'

interface DebouncedInputProps extends Omit<
  React.InputHTMLAttributes<HTMLInputElement>,
  'onChange'
> {
  value: string
  onChange: (value: string) => void
  debounceTime?: number
  icon?: React.ReactNode
  label?: string
  hideErrors?: boolean
}

export function DebouncedInput({
  value: initialValue,
  onChange,
  debounceTime = 300,
  icon,
  className = '',
  label,
  hideErrors,
  ...props
}: DebouncedInputProps) {
  const [localValue, setLocalValue] = useState(initialValue)

  useEffect(() => {
    setLocalValue(initialValue)
  }, [initialValue])

  useEffect(() => {
    if (localValue === initialValue) return

    const timer = setTimeout(() => {
      onChange(localValue)
    }, debounceTime)

    return () => clearTimeout(timer)
  }, [localValue, initialValue, debounceTime, onChange])

  return (
    <div className={`relative w-full ${className}`}>
      <Input
        {...props}
        icon={icon}
        label={label}
        value={localValue}
        onChange={(e) => setLocalValue(e.target.value)}
        className={`${icon ? 'pl-9' : ''} w-full`}
        hideErrors={hideErrors}
      />
    </div>
  )
}
