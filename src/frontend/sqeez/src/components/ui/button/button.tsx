import * as React from 'react'
import { type VariantProps } from 'class-variance-authority'
import { Slot } from 'radix-ui'
import { buttonVariants } from './buttonVariant'

import { cn } from '@/lib/utils'

export function Button({
  className,
  variant = 'default',
  size = 'default',
  asChild = false,
  ...props
}: React.ComponentProps<'button'> &
  VariantProps<typeof buttonVariants> & {
    asChild?: boolean
  }) {
  const Tag = asChild ? Slot.Root : 'button'

  return (
    <Tag
      data-slot="button"
      data-variant={variant}
      data-size={size}
      className={cn(buttonVariants({ variant, size, className }))}
      {...props}
    />
  )
}
