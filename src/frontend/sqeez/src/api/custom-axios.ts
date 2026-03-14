import axios, {
  type AxiosError,
  type AxiosRequestConfig,
  type InternalAxiosRequestConfig,
} from 'axios'
import { useAuthStore } from '@/store/useAuthStore'

interface RetryQueueItem {
  resolve: (value: unknown) => void
  reject: (error: unknown) => void
}

interface CustomAxiosRequestConfig extends InternalAxiosRequestConfig {
  _retry?: boolean
}

const baseURL =
  typeof import.meta !== 'undefined'
    ? import.meta.env.VITE_API_BASE_URL
    : 'http://localhost:5000'

export const AXIOS_INSTANCE = axios.create({
  baseURL,
  withCredentials: true,
})

let isRefreshing = false
let failedQueue: RetryQueueItem[] = []

const processQueue = (error: Error | AxiosError | null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error)
    } else {
      prom.resolve(null)
    }
  })
  failedQueue = []
}

const AUTH_ENDPOINTS = [
  '/api/auth/login',
  '/api/auth/register',
  '/api/auth/refresh',
]

AXIOS_INSTANCE.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as CustomAxiosRequestConfig

    if (!error.response || !originalRequest) {
      return Promise.reject(error)
    }

    const isUnauthorized = error.response.status === 401

    const isAuthRequest = AUTH_ENDPOINTS.some((url) =>
      originalRequest.url?.includes(url),
    )

    // Logic: Only attempt refresh if it's a 401 on a NON-auth endpoint
    if (isUnauthorized && !originalRequest._retry && !isAuthRequest) {
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject })
        })
          .then(() => AXIOS_INSTANCE(originalRequest))
          .catch((err) => Promise.reject(err))
      }

      originalRequest._retry = true
      isRefreshing = true

      try {
        await AXIOS_INSTANCE.post('/api/auth/refresh')
        processQueue(null)
        return AXIOS_INSTANCE(originalRequest)
      } catch (refreshError) {
        processQueue(refreshError as AxiosError)

        // Use the store's state directly for a clean logout
        useAuthStore.getState().setUser(null)

        return Promise.reject(refreshError)
      } finally {
        isRefreshing = false
      }
    }

    return Promise.reject(error)
  },
)

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
