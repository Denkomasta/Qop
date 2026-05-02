import { describe, expect, it } from 'vitest'
import { getImageUrl } from './imageHelpers'

describe('getImageUrl', () => {
  it('returns undefined for empty values', () => {
    expect(getImageUrl()).toBeUndefined()
    expect(getImageUrl(null)).toBeUndefined()
  })

  it('leaves absolute URLs unchanged', () => {
    expect(getImageUrl('https://example.com/image.png')).toBe(
      'https://example.com/image.png',
    )
    expect(getImageUrl('http://example.com/image.png')).toBe(
      'http://example.com/image.png',
    )
  })

  it('resolves relative paths against the API base URL', () => {
    expect(getImageUrl('/avatars/user.png')).toContain('/avatars/user.png')
  })
})
