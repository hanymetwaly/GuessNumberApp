import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import Register from '../Register'
import * as AuthContext from '../../context/AuthContext'

describe('Register page', () => {
  test('shows validation errors for invalid input', async () => {
    const registerMock = vi.fn()
    vi.spyOn(AuthContext, 'useAuth').mockReturnValue({ register: registerMock })

    render(<MemoryRouter><Register /></MemoryRouter>)

    await userEvent.type(screen.getByPlaceholderText(/Username/i), 'a')
    await userEvent.type(screen.getByPlaceholderText(/Email/i), 'not-an-email')
    await userEvent.type(screen.getByPlaceholderText(/Password/i), 'short')
    await userEvent.click(screen.getByRole('button', { name: /register/i }))

    expect(screen.getByText(/Username must be at least 3 characters/i)).toBeInTheDocument()
    expect(screen.getByText(/Enter a valid email/i)).toBeInTheDocument()
    expect(screen.getByText(/Password must be at least 6 characters/i)).toBeInTheDocument()
    expect(registerMock).not.toHaveBeenCalled()
  })

  test('calls register with valid input', async () => {
    const registerMock = vi.fn(async () => {})
    vi.spyOn(AuthContext, 'useAuth').mockReturnValue({ register: registerMock })

    render(<MemoryRouter><Register /></MemoryRouter>)

    await userEvent.type(screen.getByPlaceholderText(/Username/i), 'alice')
    await userEvent.type(screen.getByPlaceholderText(/Email/i), 'alice@example.com')
    await userEvent.type(screen.getByPlaceholderText(/Password/i), 'Secret1')
    await userEvent.click(screen.getByRole('button', { name: /register/i }))

    await waitFor(() => expect(registerMock).toHaveBeenCalledWith('alice', 'alice@example.com', 'Secret1'))
  })
})
