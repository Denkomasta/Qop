import { createFileRoute } from '@tanstack/react-router'
import { LeaderboardPage } from './-/LeaderboardPage'

export const Route = createFileRoute('/app/_authenticated/leaderboards/')({
  component: LeaderboardPage,
})
