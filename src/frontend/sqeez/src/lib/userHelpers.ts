export const calculateLevel = (xp?: number | string) => {
  if (!xp) return 1

  const parsedXp = Number(xp)

  if (Number.isNaN(parsedXp) || parsedXp < 0) return 1

  return Math.floor(parsedXp / 100) + 1
}

export const formatName = (
  firstName: string | undefined,
  lastName: string | undefined,
) => {
  return `${firstName} ${lastName}`
}

export const getNameInitials = (
  firstName: string | undefined,
  lastName: string | undefined,
) => {
  return `${firstName?.substring(0, 1) ?? 'J'}${lastName?.substring(0, 1) ?? 'D'}`
}
