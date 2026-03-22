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
