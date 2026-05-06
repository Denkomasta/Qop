import { describe, expect, it } from 'vitest'
import { toLocalDateTimeInputValue, toUtcIsoString } from './dateHelpers'

describe('dateHelpers', () => {
  it('converts datetime-local values to UTC ISO strings', () => {
    expect(toUtcIsoString('2026-06-10T14:45')).toBe(
      new Date('2026-06-10T14:45').toISOString(),
    )
  })

  it('converts date-only picker values to UTC ISO strings at local midnight', () => {
    expect(toUtcIsoString('2026-06-10')).toBe(
      new Date('2026-06-10T00:00').toISOString(),
    )
  })

  it('returns null for empty or invalid date values', () => {
    expect(toUtcIsoString('')).toBeNull()
    expect(toUtcIsoString(null)).toBeNull()
    expect(toUtcIsoString('not-a-date')).toBeNull()
  })

  it('formats UTC ISO values for datetime-local inputs', () => {
    const isoValue = '2026-05-02T10:30:00.000Z'

    expect(toLocalDateTimeInputValue(isoValue)).toBe(
      new Date(
        new Date(isoValue).getTime() -
          new Date(isoValue).getTimezoneOffset() * 60000,
      )
        .toISOString()
        .slice(0, 16),
    )
  })
})
