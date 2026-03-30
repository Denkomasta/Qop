import { useGetApiSystemConfig } from '@/api/generated/endpoints/system-config/system-config'

export function useSystemConfig() {
  const query = useGetApiSystemConfig({
    query: {
      staleTime: 1000 * 60 * 5,
      gcTime: 1000 * 60 * 60 * 24,
      refetchOnWindowFocus: false,
    },
  })

  return {
    config: query.data,
    isLoading: query.isLoading,
    isError: query.isError,
  }
}
