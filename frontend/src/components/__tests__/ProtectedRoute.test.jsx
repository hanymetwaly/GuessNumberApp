import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import ProtectedRoute from '../ProtectedRoute'
import * as AuthContext from '../../context/AuthContext'

function Dummy() {
  return <div>Secret content</div>
}

describe('ProtectedRoute', () => {
  test('renders children when user present', () => {
    vi.spyOn(AuthContext, 'useAuth').mockReturnValue({ user: { username: 'alice' } })
    render(<MemoryRouter><ProtectedRoute><Dummy/></ProtectedRoute></MemoryRouter>)

    expect(screen.getByText(/Secret content/i)).toBeInTheDocument()
  })

  test('does not render children when user absent', () => {
    vi.spyOn(AuthContext, 'useAuth').mockReturnValue({ user: null })
    render(<MemoryRouter><ProtectedRoute><Dummy/></ProtectedRoute></MemoryRouter>)

    expect(screen.queryByText(/Secret content/i)).not.toBeInTheDocument()
  })
})
