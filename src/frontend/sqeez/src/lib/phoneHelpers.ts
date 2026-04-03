export const formatPhoneForDisplay = (phone?: string | null) => {
  if (!phone) return ''

  const withPlus = phone.replace(/^00/, '+')

  return withPlus.replace(/^(\+\d{3})(\d+)/, '($1) $2')
}

export const formatPhoneForDb = (phone: string) => {
  let cleaned = phone.replace(/[\s\-()]/g, '')
  cleaned = cleaned.replace(/^\+/, '00')
  return cleaned
}
