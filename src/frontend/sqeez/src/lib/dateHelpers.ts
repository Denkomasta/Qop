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
