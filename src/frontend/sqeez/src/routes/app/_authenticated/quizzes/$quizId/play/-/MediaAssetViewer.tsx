import { useState, useEffect } from 'react'
import { AlertCircle } from 'lucide-react'
import { Spinner } from '@/components/ui/Spinner'

import { getApiMediaAssetsIdFile } from '@/api/generated/endpoints/media-assets/media-assets'

interface MediaAssetViewerProps {
  assetId: number | string
  isOption?: boolean
}

export function MediaAssetViewer({
  assetId,
  isOption = false,
}: MediaAssetViewerProps) {
  const [mediaUrl, setMediaUrl] = useState<string | null>(null)
  const [mimeType, setMimeType] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [isError, setIsError] = useState(false)

  useEffect(() => {
    let objectUrl: string | null = null

    const fetchMedia = async () => {
      try {
        setIsLoading(true)
        setIsError(false)

        const blob = await getApiMediaAssetsIdFile(assetId)

        if (!(blob instanceof Blob)) {
          throw new Error('Expected a Blob, but received something else.')
        }

        // Save the MIME type (e.g., 'image/png') to determine how to render it
        setMimeType(blob.type)

        objectUrl = URL.createObjectURL(blob)
        setMediaUrl(objectUrl)
      } catch (error) {
        console.error('Failed to load media asset', error)
        setIsError(true)
      } finally {
        setIsLoading(false)
      }
    }

    fetchMedia()

    return () => {
      if (objectUrl) {
        URL.revokeObjectURL(objectUrl)
      }
    }
  }, [assetId])

  if (isLoading) {
    return (
      <div
        className={`flex w-full items-center justify-center bg-muted/10 ${isOption ? 'h-24 rounded-md' : 'h-48 rounded-xl'}`}
      >
        <Spinner size="sm" />
      </div>
    )
  }

  if (isError || !mediaUrl) {
    return (
      <div
        className={`flex w-full flex-col items-center justify-center bg-destructive/5 text-muted-foreground ${isOption ? 'h-24 rounded-md' : 'h-48 rounded-xl'}`}
      >
        <AlertCircle className="mb-1 h-6 w-6 text-destructive opacity-50" />
        <span className="text-xs">Failed to load media</span>
      </div>
    )
  }

  if (mimeType?.startsWith('video/')) {
    return (
      <div
        className={`relative flex w-full justify-center bg-black ${isOption ? 'rounded-md' : 'overflow-hidden rounded-xl'}`}
      >
        <video
          controls
          src={mediaUrl}
          className={`w-full max-w-full ${isOption ? 'max-h-32' : 'max-h-100'}`}
        />
      </div>
    )
  }

  if (mimeType?.startsWith('audio/')) {
    return (
      <div
        className={`flex w-full items-center justify-center bg-muted/20 p-4 ${isOption ? 'rounded-md' : 'rounded-xl'}`}
      >
        <audio controls src={mediaUrl} className="w-full max-w-md" />
      </div>
    )
  }

  return (
    <div
      className={`flex w-full justify-center bg-muted/5 ${isOption ? 'rounded-md' : 'rounded-xl'}`}
    >
      <img
        src={mediaUrl}
        alt={`Media asset ${assetId}`}
        className={`object-contain ${isOption ? 'max-h-32 rounded-md' : 'max-h-100 rounded-xl'}`}
        loading="lazy"
      />
    </div>
  )
}
