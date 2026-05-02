import { describe, expect, it } from 'vitest'
import { calculateLevel, formatName, getNameInitials } from './userHelpers'

describe('userHelpers', () => {
  describe('calculateLevel', () => {
    it('returns level one for missing, invalid and negative XP', () => {
      expect(calculateLevel()).toBe(1)
      expect(calculateLevel('not-a-number')).toBe(1)
      expect(calculateLevel(-10)).toBe(1)
    })

    it('calculates levels from numeric and string XP values', () => {
      expect(calculateLevel(0)).toBe(1)
      expect(calculateLevel(1000)).toBeGreaterThan(1)
      expect(calculateLevel('1000')).toBe(calculateLevel(1000))
    })
  })

  it('formats first and last name values', () => {
    expect(formatName('Dana', 'User')).toBe('Dana User')
    expect(formatName(undefined, 'User')).toBe('undefined User')
  })

  it('creates initials with sensible fallbacks', () => {
    expect(getNameInitials('Dana', 'User')).toBe('DU')
    expect(getNameInitials(undefined, undefined)).toBe('JD')
    expect(getNameInitials('Dana', undefined)).toBe('DD')
  })
})
