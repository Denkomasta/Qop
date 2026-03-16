import { getNameInitials } from '@/lib/userHelpers'
import { Avatar, AvatarFallback, AvatarImage } from './Avatar'
import { cn } from '@/lib/utils'

interface SimpleAvatarProps {
  url?: string | null
  username?: string
  firstName?: string
  lastName?: string
  wrapperClassName?: string
  imageClassName?: string
  fallbackClassName?: string
}

export const SimpleAvatar = ({
  url,
  username,
  firstName,
  lastName,
  wrapperClassName,
  imageClassName,
  fallbackClassName,
}: SimpleAvatarProps) => {
  const initials = getNameInitials(firstName, lastName)

  return (
    <Avatar className={cn('border-2', wrapperClassName)}>
      {url ? (
        <AvatarImage
          src={url}
          alt={`${username ?? initials}'s avatar`}
          className={imageClassName}
        />
      ) : (
        <AvatarFallback className={fallbackClassName}>
          {firstName
            ? initials
            : (username?.substring(0, 2).toUpperCase() ?? 'JD')}
        </AvatarFallback>
      )}
    </Avatar>
  )
}
