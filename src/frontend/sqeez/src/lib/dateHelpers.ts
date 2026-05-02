import type { TFunction } from 'i18next'

export const formatDate = (dateString?: string | null) => {
  if (!dateString) return null
  return new Date(dateString).toLocaleDateString()
}

/**
 * Formats an ISO date string into a localized medium date and short time format.
 * Defaults to the user's system locale.
 * * @param dateString - The date string to format.
 * @returns The formatted string, or null if the input is empty or invalid.
 */
export const formatDateTime = (dateString?: string | null): string | null => {
  if (!dateString) return null

  const date = new Date(dateString)

  if (isNaN(date.getTime())) return null

  return date.toLocaleString([], {
    dateStyle: 'medium',
    timeStyle: 'short',
  })
}

/**
 * Formats an ISO date string into a localized medium date format (no time).
 */
export const formatDateOnly = (dateString?: string | null): string | null => {
  if (!dateString) return null

  const date = new Date(dateString)

  if (isNaN(date.getTime())) return null

  return date.toLocaleDateString([], {
    dateStyle: 'medium',
  })
}

export const formatDuration = (start: string | null, end: string | null) => {
  if (!start || !end) return '-'

  const startTime = new Date(start).getTime()
  const endTime = new Date(end).getTime()

  const diffInSeconds = Math.floor((endTime - startTime) / 1000)

  if (diffInSeconds < 0) return '-'

  const minutes = Math.floor(diffInSeconds / 60)
  const seconds = diffInSeconds % 60

  return minutes > 0 ? `${minutes}m ${seconds}s` : `${seconds}s`
}

/**
 * Formats a duration in seconds into a digital stopwatch format (e.g., 65 -> "1:05").
 * Useful for live counters and timers.
 * @param totalSeconds - The total number of elapsed seconds.
 * @returns The formatted string.
 */
export const formatTimer = (totalSeconds: number): string => {
  if (totalSeconds < 0) return '0:00'

  const m = Math.floor(totalSeconds / 60)
  const s = totalSeconds % 60

  return `${m}:${s.toString().padStart(2, '0')}`
}

export const getLastSeenStatus = (
  lastSeen: string | undefined,
  t: TFunction,
) => {
  // If lastSeen is null/undefined or invalid, fallback to offline
  if (!lastSeen) return { isOnline: false, text: t('profile.offline') }

  const lastSeenDate = new Date(lastSeen)
  const now = new Date()

  const diffMs = now.getTime() - lastSeenDate.getTime()
  const diffMinutes = Math.floor(diffMs / (1000 * 60))

  // Less than 5 minutes = Online
  if (diffMinutes < 5) {
    return { isOnline: true, text: t('profile.online') }
  }

  // Format relative time for offline status
  if (diffMinutes < 60) {
    return {
      isOnline: false,
      text: t('profile.lastSeen.minutes', {
        count: diffMinutes,
      }),
    }
  }

  const diffHours = Math.floor(diffMinutes / 60)
  if (diffHours < 24) {
    return {
      isOnline: false,
      text: t('profile.lastSeen.hours', {
        count: diffHours,
      }),
    }
  }

  const diffDays = Math.floor(diffHours / 24)
  return {
    isOnline: false,
    text: t('profile.lastSeen.days', {
      count: diffDays,
    }),
  }
}
