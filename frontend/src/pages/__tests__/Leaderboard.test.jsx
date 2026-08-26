import { render, screen, waitFor } from '@testing-library/react'
import Leaderboard from '../Leaderboard'

describe('Leaderboard page', () => {
  test('renders loading then table rows', async () => {
    render(<Leaderboard />)

    expect(screen.getByText(/Loading/i)).toBeInTheDocument()

    await waitFor(() => expect(screen.getByText(/alice/i)).toBeInTheDocument())
    expect(screen.getByText(/bob/i)).toBeInTheDocument()
    expect(screen.getByText(/2/)).toBeInTheDocument()
  })
})
