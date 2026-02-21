import { createFileRoute } from '@tanstack/react-router'

import { useState } from 'react'
import { Button } from '@/components/ui'
import { ThemeSwitcher } from '@/components/settings/ThemeSwitcher/ThemeSwitcher'

export const Route = createFileRoute('/')({
  component: Landing,
})

function Landing() {
  const [count, setCount] = useState(0)

  return (
    <>
      <h1 className="text-4xl font-bold text-blue-800">Vite + React</h1>
      <div className="card w-96 bg-base-100 shadow-sm card-xs">
        <div className="card-body">
          <h2 className="card-title text-black">Xsmall Card</h2>
          <p>
            A card component has a figure, a body part, and inside body there
            are title and actions parts
          </p>
          <div className="card-actions justify-end">
            <Button
              onClick={() => setCount((count) => count + 1)}
              className="btn btn-primary"
              variant={'ghost'}
            >
              count is {count}
            </Button>
          </div>
          <div className="mt-2">
            <Button variant={'outline'}>{'vejceeee'}</Button>
            <ThemeSwitcher />
          </div>
        </div>
      </div>
    </>
  )
}
