# GuessNumberApp — Frontend

A React + Vite single-page app for the "Guess the Number" game. It talks to the
[GuessNumber.Api](../README.md) backend for authentication, gameplay, and the
leaderboard.

## Tech stack
- React 19 + Vite
- React Router (`react-router-dom`) for routing
- Axios for API calls (with JWT + error-normalizing interceptors)
- Context API for auth state (`AuthContext`)
- Vitest + Testing Library + msw for unit tests

## Prerequisites
- Node.js 18+ (or the version your team standardizes on)
- The backend API running and reachable (see the root README)

## Setup
```bash
cd frontend
npm install
```

> Note: this project uses React 19 with `@testing-library/react@16`. If you add
> dependencies that still peer-depend on React 18, npm may report an
> `ERESOLVE` conflict — install those with `--legacy-peer-deps`.

## Environment variables
Create a `.env` (or `.env.local`) file in `frontend/`:

```bash
# Base URL of the backend API — include the /api prefix
VITE_API_URL=http://localhost:5000/api
```

The Axios client (`src/api/client.js`) uses `VITE_API_URL` as its `baseURL` and
issues requests to paths like `/auth/login` and `/game/start`, so the value must
include the API's `/api` prefix.

## Scripts
| Command | Description |
| --- | --- |
| `npm run dev` | Start the Vite dev server with HMR |
| `npm run build` | Production build to `dist/` |
| `npm run preview` | Preview the production build locally |
| `npm run lint` | Run Oxlint |
| `npm test` | Run the unit test suite (Vitest) |

Run tests once (non-watch) with:

```bash
npm test -- --run
```

## Routes
| Path | Page | Access |
| --- | --- | --- |
| `/` | Game | Protected (requires login) |
| `/login` | Login | Public |
| `/register` | Register | Public |
| `/leaderboard` | Leaderboard | Public |
| `*` | Redirects to `/` | — |

## Project structure
```
src/
  api/client.js         Axios instance, JWT + error interceptors
  context/AuthContext.jsx  Auth state (register/login/logout/updateBestScore)
  components/
    NavBar.jsx
    ProtectedRoute.jsx  Guards routes that require a logged-in user
  pages/
    Game.jsx
    Leaderboard.jsx
    Login.jsx
    Register.jsx
  mocks/                msw handlers + server used by tests
  setupTests.js         Test bootstrap (jest-dom, msw lifecycle)
  App.jsx               Router + provider composition
  main.jsx              App entry point
```

## Testing
Unit tests live in `__tests__/` folders next to the code they cover and run on
[Vitest](https://vitest.dev/) in a `jsdom` environment.

- **Testing Library** (`@testing-library/react`, `user-event`) for rendering and
  interaction.
- **msw** intercepts HTTP calls so components hit realistic mock responses
  (see `src/mocks/handlers.js`).
- Global test config lives in `vite.config.js` under the `test` key
  (`globals`, `environment: 'jsdom'`, `setupFiles`).

Components that use router hooks (`useNavigate`, `<Navigate>`, `<Link>`) must be
rendered inside a router in tests — wrap them in `<MemoryRouter>`.
