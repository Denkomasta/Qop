import { createFileRoute } from '@tanstack/react-router'

import { useState } from 'react'
import reactLogo from '@/assets/react.svg'
import viteLogo from '/vite.svg'
import { Button } from '@/components/ui'

export const Route = createFileRoute('/')({
  component: Landing,
})

function Landing() {
  const [count, setCount] = useState(0)

  return (
    <>
      <div>
        <a href="https://vite.dev" target="_blank">
          <img src={viteLogo} className="logo" alt="Vite logo" />
        </a>
        <a href="https://react.dev" target="_blank">
          <img src={reactLogo} className="logo react" alt="React logo" />
        </a>
      </div>
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
        </div>
      </div>
    </>
  )
}
