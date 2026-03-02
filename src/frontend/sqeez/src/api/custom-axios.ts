import axios, { type AxiosRequestConfig, AxiosError } from 'axios'

const baseURL =
  typeof import.meta !== 'undefined'
    ? import.meta.env.VITE_API_BASE_URL
    : 'http://localhost:5000'

export const AXIOS_INSTANCE = axios.create({ baseURL, withCredentials: true })

export const customInstance = <T>(
  config: AxiosRequestConfig,
  options?: AxiosRequestConfig,
): Promise<T> => {
  const source = axios.CancelToken.source()
  const promise = AXIOS_INSTANCE({
    ...config,
    ...options,
    cancelToken: source.token,
  }).then(({ data }) => data)

  // @ts-expect-error - We are adding a custom cancel method to the promise
  promise.cancel = () => {
    source.cancel('Query was cancelled')
  }

  return promise
}

export type ErrorType<Error> = AxiosError<Error>
