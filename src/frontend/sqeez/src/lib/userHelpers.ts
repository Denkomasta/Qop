export const calculateLevel = (xp?: number | string) => {
  if (!xp) return 1

  const parsedXp = Number(xp)

  if (Number.isNaN(parsedXp) || parsedXp < 0) return 1

  // Multiplier: adjusts the overall speed (higher number = faster leveling)
  const MULTIPLIER = 0.05

  // Exponent: determines the size of the "jumps" between levels.
  // 0.5 = huge jumps (the previous square root version)
  // 1.0 = linear (every level requires the exact same amount of XP)
  // 0.8 = ideal compromise, jumps grow smoothly and more slowly
  const EXPONENT = 0.85

  // Level = floor of (multiplier * XP^exponent) + 1
  return Math.floor(MULTIPLIER * Math.pow(parsedXp, EXPONENT)) + 1
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
