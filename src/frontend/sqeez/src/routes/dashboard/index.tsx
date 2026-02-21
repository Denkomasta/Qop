import SqeezLogo from '@/components/icons/logos/SqeezLogo'
import { createFileRoute } from '@tanstack/react-router'

export const Route = createFileRoute('/dashboard/')({
  component: Dashboard,
})

function Dashboard() {
  return (
    <div className="dashboard-container">
      <SqeezLogo size={200} className="dashboard-logo" />
    </div>
  )
}
