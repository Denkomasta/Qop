export function getImageUrl(path?: string | null): string | undefined {
  if (!path) return undefined

  if (path.startsWith('http://') || path.startsWith('https://')) {
    return path
  }

  const baseUrl = import.meta.env.VITE_API_BASE_URL || ''

  const cleanBaseUrl = baseUrl.replace(/\/$/, '')
  const cleanPath = path.replace(/^\//, '')

  return `${cleanBaseUrl}/${cleanPath}`
}
