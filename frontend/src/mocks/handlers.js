import { rest } from 'msw'

const API = import.meta.env.VITE_API_URL || ''

export const handlers = [
  rest.post(`${API}/game/start`, (req, res, ctx) => {
    return res(ctx.status(200), ctx.json({ gameId: 'game-1', message: "I'm thinking of a number between 1 and 43. Take a guess!" }))
  }),

  rest.post(`${API}/game/guess`, async (req, res, ctx) => {
    const body = await req.json()
    const g = body.guess
    if (g === 10) return res(ctx.status(200), ctx.json({ result: 'correct', attempts: 1, isNewRecord: true, bestScore: 1 }))
    if (g < 10) return res(ctx.status(200), ctx.json({ result: 'higher', attempts: 1 }))
    return res(ctx.status(200), ctx.json({ result: 'lower', attempts: 1 }))
  }),

  rest.get(`${API}/game/leaderboard`, (req, res, ctx) => {
    return res(ctx.status(200), ctx.json([
      { rank: 1, username: 'alice', bestScore: 2 },
      { rank: 2, username: 'bob', bestScore: 3 }
    ]))
  }),

  rest.post(`${API}/auth/login`, async (req, res, ctx) => {
    const body = await req.json()
    if (body.username === 'bad') return res(ctx.status(400), ctx.json({ title: 'Invalid credentials' }))
    // In production, server should set an httpOnly cookie instead of returning token.
    return res(ctx.status(200), ctx.json({ token: 'fake-jwt', username: body.username, bestScore: null }))
  }),

  rest.post(`${API}/auth/register`, async (req, res, ctx) => {
    const body = await req.json()
    if (body.username === 'exists') return res(ctx.status(409), ctx.json({ title: 'User already exists' }))
    return res(ctx.status(201), ctx.json({ token: 'fake-jwt', username: body.username, bestScore: null }))
  })
]
