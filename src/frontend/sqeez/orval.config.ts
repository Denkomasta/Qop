import { defineConfig } from 'orval'

export default defineConfig({
  petstore: {
    input: './src/api/api.yaml',
    output: {
      mode: 'tags-split',
      target: './src/api/generated/endpoints',
      schemas: './src/api/generated/model',
      client: 'react-query',
      httpClient: 'axios',
      tsconfig: './tsconfig.app.json',
      override: {
        mutator: {
          path: './src/api/custom-axios.ts',
          name: 'customInstance',
        },
        enumGenerationType: 'union',
      },
    },
    hooks: {
      afterAllFilesWrite: 'prettier --write src/api/generated',
    },
  },
})
