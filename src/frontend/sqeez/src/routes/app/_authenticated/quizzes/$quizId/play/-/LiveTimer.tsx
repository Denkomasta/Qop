import { useState, useEffect } from 'react'
import { Clock } from 'lucide-react'
import { formatTimer } from '@/lib/dateHelpers'

export function LiveTimer() {
  const [elapsedSeconds, setElapsedSeconds] = useState(0)

  useEffect(() => {
    const intervalId = setInterval(() => {
      setElapsedSeconds((prev) => prev + 1)
    }, 1000)

    return () => clearInterval(intervalId)
  }, [])

  return (
    <div className="flex items-center gap-1.5 text-foreground/80">
      <Clock className="h-4 w-4" />
      <span className="w-10 text-right tabular-nums">
        {formatTimer(elapsedSeconds)}
      </span>
    </div>
  )
}
