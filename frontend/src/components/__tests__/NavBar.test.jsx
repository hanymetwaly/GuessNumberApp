import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import NavBar from '../NavBar'
import * as AuthContext from '../../context/AuthContext'

const { navigateMock } = vi.hoisted(() => ({ navigateMock: vi.fn() }))

vi.mock('react-router-dom', async (importOriginal) => {
  const actual = await importOriginal()
  return { ...actual, useNavigate: () => navigateMock }
})

describe('NavBar', () => {
  beforeEach(() => {
    navigateMock.mockClear()
  })

  test('shows login/register when not authenticated', () => {
    vi.spyOn(AuthContext, 'useAuth').mockReturnValue({ user: null })
    render(<MemoryRouter><NavBar /></MemoryRouter>)

    expect(screen.getByText(/Login/i)).toBeInTheDocument()
    expect(screen.getByText(/Register/i)).toBeInTheDocument()
  })

  test('shows user and logout when authenticated and handles logout', async () => {
    const logoutMock = vi.fn()
    vi.spyOn(AuthContext, 'useAuth').mockReturnValue({ user: { username: 'alice' }, logout: logoutMock })

    render(<MemoryRouter><NavBar /></MemoryRouter>)

    expect(screen.getByText(/Hi, alice/i)).toBeInTheDocument()

    await userEvent.click(screen.getByRole('button', { name: /logout/i }))

    expect(logoutMock).toHaveBeenCalled()
    expect(navigateMock).toHaveBeenCalledWith('/login')
  })
})
