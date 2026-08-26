import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import Login from '../Login'
import * as AuthContext from '../../context/AuthContext'

describe('Login page', () => {
  test('shows validation errors for empty fields', async () => {
    const loginMock = vi.fn()
    vi.spyOn(AuthContext, 'useAuth').mockReturnValue({ login: loginMock })

    render(<MemoryRouter><Login /></MemoryRouter>)

    await userEvent.click(screen.getByRole('button', { name: /login/i }))

    expect(screen.getByText(/Username is required/i)).toBeInTheDocument()
    expect(screen.getByText(/Password is required/i)).toBeInTheDocument()
    expect(loginMock).not.toHaveBeenCalled()
  })

  test('calls login with provided credentials', async () => {
    const loginMock = vi.fn(async () => {})
    vi.spyOn(AuthContext, 'useAuth').mockReturnValue({ login: loginMock })

    render(<MemoryRouter><Login /></MemoryRouter>)

    await userEvent.type(screen.getByPlaceholderText(/Username/i), 'alice')
    await userEvent.type(screen.getByPlaceholderText(/Password/i), 'Secret1')
    await userEvent.click(screen.getByRole('button', { name: /login/i }))

    await waitFor(() => expect(loginMock).toHaveBeenCalledWith('alice', 'Secret1'))
  })
})
