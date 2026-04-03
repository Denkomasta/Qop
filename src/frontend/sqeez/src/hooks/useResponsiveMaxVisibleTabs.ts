import { useState, useLayoutEffect } from 'react'

export function useResponsiveMaxVisible() {
  const [maxVisible, setMaxVisible] = useState(3)

  useLayoutEffect(() => {
    const handleResize = () => {
      const width = window.innerWidth
      if (width >= 1280) {
        setMaxVisible(6)
      } else if (width >= 1024) {
        setMaxVisible(4)
      } else {
        setMaxVisible(2)
      }
    }

    handleResize()

    window.addEventListener('resize', handleResize)
    return () => window.removeEventListener('resize', handleResize)
  }, [])

  return maxVisible
}
