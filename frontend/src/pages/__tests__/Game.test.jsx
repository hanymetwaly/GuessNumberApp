import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import Game from '../Game'
import { vi } from 'vitest'

// Mock confetti to avoid side effects
vi.mock('canvas-confetti', () => ({ default: vi.fn() }))

// Shared spy so tests can assert calls to updateBestScore
const { updateBestScore } = vi.hoisted(() => ({ updateBestScore: vi.fn() }))

// Mock Auth context to provide user and updateBestScore
vi.mock('../../context/AuthContext', () => ({
  useAuth: () => ({ user: { username: 'alice', bestScore: null }, updateBestScore })
}))

describe('Game page', () => {
  beforeEach(() => {
    updateBestScore.mockClear()
  })

  test('start game shows form and message', async () => {
    render(<Game />)

    const startBtn = screen.getByRole('button', { name: /start game/i })
    await userEvent.click(startBtn)

    await waitFor(() => expect(screen.getByPlaceholderText(/1-43/i)).toBeInTheDocument())
    expect(screen.getByText(/I'm thinking of a number/i)).toBeInTheDocument()
  })

  test('submit guess shows higher/lower and updates attempts', async () => {
    render(<Game />)

    // start
    await userEvent.click(screen.getByRole('button', { name: /start game/i }))
    await waitFor(() => screen.getByPlaceholderText(/1-43/i))

    // guess lower than secret (5 -> higher)
    await userEvent.type(screen.getByPlaceholderText(/1-43/i), '5')
    await userEvent.click(screen.getByRole('button', { name: /guess/i }))

    await waitFor(() => screen.getByText(/go higher/i))
    expect(screen.getByText(/Attempts: 1/i)).toBeInTheDocument()

    // guess higher than secret (20 -> lower)
    await userEvent.type(screen.getByPlaceholderText(/1-43/i), '20')
    await userEvent.click(screen.getByRole('button', { name: /guess/i }))
    await waitFor(() => screen.getByText(/go lower/i))
  })

  test('correct guess shows record and calls updateBestScore', async () => {
    render(<Game />)

    await userEvent.click(screen.getByRole('button', { name: /start game/i }))
    await waitFor(() => screen.getByPlaceholderText(/1-43/i))

    await userEvent.type(screen.getByPlaceholderText(/1-43/i), '10')
    await userEvent.click(screen.getByRole('button', { name: /guess/i }))

    await waitFor(() => screen.getByText(/NEW RECORD/i))
    expect(updateBestScore).toHaveBeenCalledWith(1)
  })
})
