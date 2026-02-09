import 'i18next'
import { ParseKeys } from 'i18next'
import translation from '../public/locales/en/translation.json'

export type TranslationKey = ParseKeys

declare module 'i18next' {
  interface CustomTypeOptions {
    defaultNS: 'translation'
    resources: {
      translation: typeof translation
    }
  }
}
