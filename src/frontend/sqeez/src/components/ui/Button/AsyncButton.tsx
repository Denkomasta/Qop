import { useState } from 'react'
import { Button, type ButtonProps } from '@/components/ui/Button'
import { Spinner } from '../Spinner'

interface AsyncButtonProps extends Omit<ButtonProps, 'onClick' | 'asChild'> {
  onClick?: (
    e: React.MouseEvent<HTMLButtonElement, MouseEvent>,
  ) => Promise<void> | void
  loadingText?: string
}

export function AsyncButton({
  onClick,
  children,
  disabled,
  loadingText,
  ...props
}: AsyncButtonProps) {
  const [isInternalLoading, setIsInternalLoading] = useState(false)

  const handleClick = async (
    e: React.MouseEvent<HTMLButtonElement, MouseEvent>,
  ) => {
    if (!onClick) return

    setIsInternalLoading(true)
    try {
      await onClick(e)
    } finally {
      setIsInternalLoading(false)
    }
  }

  return (
    <Button
      disabled={isInternalLoading || disabled}
      onClick={handleClick}
      {...props}
    >
      {isInternalLoading && <Spinner size={'sm'} />}
      {isInternalLoading && loadingText ? loadingText : children}
    </Button>
  )
}
