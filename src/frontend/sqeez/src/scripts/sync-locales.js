/* eslint-env node */
import fs from 'node:fs'
import path from 'node:path'

const LOCALES_DIR = './public/locales'
const OUTPUT_FILE = './public/locales/languages.json'

const LANGUAGE_MAP = {
  en: { label: 'English', flag: '🇺🇸' },
  es: { label: 'Español', flag: '🇪🇸' },
  fr: { label: 'Français', flag: '🇫🇷' },
  cs: { label: 'Čeština', flag: '🇨🇿' },
}

try {
  const folders = fs
    .readdirSync(LOCALES_DIR)
    .filter((f) => fs.lstatSync(path.join(LOCALES_DIR, f)).isDirectory())

  const manifest = folders.map((code) => ({
    code,
    label: LANGUAGE_MAP[code]?.label || code.toUpperCase(),
    flag: LANGUAGE_MAP[code]?.flag || '🌐',
  }))

  fs.writeFileSync(OUTPUT_FILE, JSON.stringify(manifest, null, 2))

  console.log(
    `\x1b[32m%s\x1b[0m`,
    `✔ Successfully synced ${manifest.length} languages.`,
  )
} catch (error) {
  console.error('❌ Error syncing locales:', error.message)
  process.exit(1)
}
