import React, { useState } from 'react'
import { Input } from '@/components/ui/Input'

interface DateTimePickerProps extends Omit<
  React.ComponentProps<typeof Input>,
  'value' | 'onChange' | 'min' | 'max'
> {
  value?: string | null
  min?: string | null
  max?: string | null
  onChange: (isoString: string | null) => void
}

export function DateTimePicker({
  value,
  onChange,
  min,
  max,
  icon,
  ...props
}: DateTimePickerProps) {
  const toLocalDatetimeString = (isoString?: string | null) => {
    if (!isoString) return ''
    try {
      const date = new Date(isoString)
      if (isNaN(date.getTime())) return ''
      return new Date(date.getTime() - date.getTimezoneOffset() * 60000)
        .toISOString()
        .slice(0, 16)
    } catch {
      return ''
    }
  }

  const [prevValueProp, setPrevValueProp] = useState(value)
  const [localValue, setLocalValue] = useState(toLocalDatetimeString(value))

  if (value !== prevValueProp) {
    setPrevValueProp(value)
    setLocalValue(toLocalDatetimeString(value))
  }

  const handleBlur = () => {
    if (!localValue) {
      if (value !== null) onChange(null)
      return
    }

    try {
      const utcDate = new Date(localValue).toISOString()

      if (utcDate !== value) {
        onChange(utcDate)
      }
    } catch {
      // Ignore gracefully if they clicked away while the date was invalid
    }
  }

  return (
    <Input
      type="datetime-local"
      icon={icon}
      value={localValue}
      onChange={(e) => setLocalValue(e.target.value)}
      onBlur={handleBlur}
      min={min ? toLocalDatetimeString(min) : undefined}
      max={max ? toLocalDatetimeString(max) : undefined}
      {...props}
    />
  )
}
