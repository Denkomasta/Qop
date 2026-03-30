export interface QuizDateInfo {
  publishDate?: string | null
  closingDate?: string | null
}

/**
 * Ensures a date string is parsed as UTC.
 * If your .NET API strips the trailing 'Z', this adds it back
 * so the browser doesn't mistakenly parse it as local time.
 */
const parseUtcTime = (dateString: string): number => {
  const safeString = dateString.endsWith('Z') ? dateString : `${dateString}Z`
  return new Date(safeString).getTime()
}

/**
 * Checks if a quiz is currently active and available to be played.
 */
export function isQuizActive(quiz?: QuizDateInfo | null): boolean {
  if (!quiz || !quiz.publishDate) return false

  const now = Date.now()
  const publishTime = parseUtcTime(quiz.publishDate)

  if (now < publishTime) {
    return false
  }

  if (quiz.closingDate) {
    const closingTime = parseUtcTime(quiz.closingDate)
    if (now > closingTime) {
      return false
    }
  }

  return true
}

/**
 * Returns a specific status string for UI badges.
 * Returns: 'draft' | 'scheduled' | 'active' | 'closed'
 */
export function getQuizStatus(
  quiz?: QuizDateInfo | null,
): 'draft' | 'scheduled' | 'active' | 'closed' {
  if (!quiz || !quiz.publishDate) return 'draft'

  const now = Date.now()
  const publishTime = parseUtcTime(quiz.publishDate)

  if (now < publishTime) return 'scheduled'

  if (quiz.closingDate) {
    const closingTime = parseUtcTime(quiz.closingDate)
    if (now > closingTime) return 'closed'
  }

  return 'active'
}
