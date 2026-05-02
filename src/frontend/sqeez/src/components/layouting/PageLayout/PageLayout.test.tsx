import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { PageLayout } from './PageLayout'

describe('PageLayout', () => {
  it('renders loading state', () => {
    render(<PageLayout isLoading>Content</PageLayout>)

    expect(screen.getByText('common.loading...')).toBeInTheDocument()
    expect(screen.queryByText('Content')).not.toBeInTheDocument()
  })

  it('renders header content and children', () => {
    render(
      <PageLayout
        title="Dashboard"
        titleBadge="Beta"
        subtitle="Overview"
        headerActions={<button>Create</button>}
        headerControls={<input aria-label="Filter" />}
      >
        Main content
      </PageLayout>,
    )

    expect(
      screen.getByRole('heading', { name: 'Dashboard' }),
    ).toBeInTheDocument()
    expect(screen.getByText('Beta')).toBeInTheDocument()
    expect(screen.getByText('Overview')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Create' })).toBeInTheDocument()
    expect(screen.getByLabelText('Filter')).toBeInTheDocument()
    expect(screen.getByText('Main content')).toBeInTheDocument()
  })
})
