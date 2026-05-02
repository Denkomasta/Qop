import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { DataTable } from './DataTable'

interface Row {
  id: number
  name: string
}

const columns = [
  { header: 'Name', cell: (row: Row) => row.name },
  { header: 'ID', cell: (row: Row) => row.id },
]

describe('DataTable', () => {
  it('renders headers and rows', () => {
    render(
      <DataTable
        data={[{ id: 1, name: 'Dana' }]}
        columns={columns}
        keyExtractor={(row) => row.id}
      />,
    )

    expect(
      screen.getByRole('columnheader', { name: 'Name' }),
    ).toBeInTheDocument()
    expect(screen.getByText('Dana')).toBeInTheDocument()
    expect(screen.getByText('1')).toBeInTheDocument()
  })

  it('renders an empty message', () => {
    render(
      <DataTable
        data={[]}
        columns={columns}
        keyExtractor={(row) => row.id}
        emptyMessage="Nothing here"
      />,
    )

    expect(screen.getByText('Nothing here')).toBeInTheDocument()
  })

  it('renders a loading row', () => {
    const { container } = render(
      <DataTable
        data={[]}
        columns={columns}
        keyExtractor={(row) => row.id}
        isLoading
      />,
    )

    expect(container.querySelector('.animate-spin')).toBeInTheDocument()
  })
})
