import '@testing-library/jest-dom'
import { server } from './mocks/server'

// Establish API mocking before all tests.
beforeAll(() => server.listen({ onUnhandledRequest: 'warn' }))
// Reset any runtime request handlers we may add during the tests.
afterEach(() => server.resetHandlers())
// Clean up once the tests are done.
afterAll(() => server.close())
