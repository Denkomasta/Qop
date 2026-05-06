/* eslint-env node */
import fs from 'node:fs'
import path from 'node:path'

const LOCALES_DIR = './public/locales'
const CHECK_ONLY = process.argv.includes('--check')

const sortJsonValue = (value) => {
  if (Array.isArray(value)) {
    return value.map(sortJsonValue)
  }

  if (value && typeof value === 'object') {
    return Object.keys(value)
      .sort((a, b) => a.localeCompare(b))
      .reduce((sortedValue, key) => {
        sortedValue[key] = sortJsonValue(value[key])
        return sortedValue
      }, {})
  }

  return value
}

const getTranslationFiles = () => {
  return fs
    .readdirSync(LOCALES_DIR, { withFileTypes: true })
    .filter((entry) => entry.isDirectory())
    .map((entry) => path.join(LOCALES_DIR, entry.name, 'translation.json'))
    .filter((filePath) => fs.existsSync(filePath))
}

const normalizeJson = (value) => `${JSON.stringify(value, null, 2)}\n`

try {
  const files = getTranslationFiles()
  const unsortedFiles = []

  files.forEach((filePath) => {
    const currentContent = fs.readFileSync(filePath, 'utf8')
    const sortedContent = normalizeJson(
      sortJsonValue(JSON.parse(currentContent)),
    )

    if (currentContent !== sortedContent) {
      unsortedFiles.push(filePath)

      if (!CHECK_ONLY) {
        fs.writeFileSync(filePath, sortedContent)
      }
    }
  })

  if (unsortedFiles.length === 0) {
    console.log(`Locale keys are sorted in ${files.length} files.`)
    process.exit(0)
  }

  if (CHECK_ONLY) {
    console.error('Locale keys are not sorted:')
    unsortedFiles.forEach((filePath) => console.error(`- ${filePath}`))
    process.exit(1)
  }

  console.log(`Sorted locale keys in ${unsortedFiles.length} files.`)
} catch (error) {
  console.error('Error sorting locales:', error.message)
  process.exit(1)
}
