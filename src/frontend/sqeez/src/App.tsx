import { useState } from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from '/vite.svg'

function App() {
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
        <h1 className='text-blue-800 text-4xl font-bold'>Vite + React</h1>
        <div className="card w-96 bg-base-100 card-xs shadow-sm">
        <div className="card-body">
          <h2 className="card-title">Xsmall Card</h2>
          <p>A card component has a figure, a body part, and inside body there are title and actions parts</p>
          <div className="justify-end card-actions">
            <button onClick={() => setCount((count) => count + 1)} className="btn btn-primary">count is {count}</button>
          </div>
        </div>
      </div>
    </>
  )
}

export default App
