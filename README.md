# GuessNumberApp

A small .NET 10 web API and domain project for a "guess the number" game.

**GitHub:** https://github.com/hanymetwaly/GuessNumberApp  
**Live demo:** https://guessnumber-demo.westus2.cloudapp.azure.com

This is the fastest way to get the application running on your own machine for review or development.

## 1. Run the full stack locally

### Prerequisites
- .NET 10 SDK
- Node.js 18+
- Docker (for the Postgres database)

### Step 1 — Start Postgres

If you already have Postgres running, create a database named `guessnumber` and ensure the connection string in `backend/GuessNumber.Api/appsettings.Development.json` is correct.

Otherwise, run Postgres in Docker with one command:

```bash
docker run -d \
  --name guessnumber-db \
  -e POSTGRES_DB=guessnumber \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -p 5432:5432 \
  postgres:16
```

### Step 2 — Clone the repository

```bash
git clone https://github.com/hanymetwaly/GuessNumberApp.git
cd GuessNumberApp
```

### Step 3 — Run the backend

```bash
cd backend/GuessNumber.Api
dotnet run
```

The API starts on `http://localhost:5000` and Swagger UI is available at `http://localhost:5000/swagger`.

### Step 4 — Run the frontend

Open a new terminal:

```bash
cd frontend
npm install
echo "VITE_API_URL=http://localhost:5000/api" > .env.local
npm run dev
```

Vite prints the local URL (usually `http://localhost:5173`). Open it in your browser.

### Step 5 — Use the app
- Register an account at `/register`.
- Log in at `/login`.
- Play the game at `/`.
- View the leaderboard at `/leaderboard`.

## 2. Run tests

### Backend tests

```bash
cd backend/GuessNumber.Tests
dotnet test
```

### Frontend tests

```bash
cd frontend
npm install
npm test -- --run
```

## Solution layout
Root: GuessNumberApp.slnx

- `backend/GuessNumber.Api` — ASP.NET Core Web API
  - `Program.cs` — app startup, DI, middleware, authentication, CORS, Swagger
  - Controllers — HTTP endpoints (Auth, Games, etc.)
  - `appsettings.json` / `appsettings.Development.json` — configuration (Jwt, ConnectionStrings, CORS)

- `backend/GuessNumber.Application` — Application services and DTOs
  - DTOs (e.g. `RegisterDto`, `LoginDto`, `AuthResponseDto`, `GameDto`)
  - Service interfaces (use-cases), mapping code and orchestration logic

- `backend/GuessNumber.Domain` — Domain entities and business rules
  - Entities like `Game`, `User` and domain logic
  - Keep domain behavior here (invariants, validations that are domain-specific)

- `backend/GuessNumber.Infrastructure` — Persistence and external integrations
  - `AppDbContext` (EF Core), repository implementations
  - Persistence migrations and DB provider configuration

- `frontend/` — React + Vite single-page app (game UI, auth, leaderboard)
  - Consumes the API above; see [`frontend/README.md`](frontend/README.md) for details

## Design principles
- Single Responsibility: each layer has a single focus
- Dependency direction: outer layers depend on inner layers; domain has no framework dependencies
- DTOs at the boundary: map domain entities to DTOs before returning to the client
- Avoid leaking sensitive fields (e.g., `SecretNumber`) in API responses

## Database and Migrations
- Connection strings live in `backend/GuessNumber.Api/appsettings*.json`.
- On startup this project applies EF Core migrations automatically (see `Program.cs`).

Only run these manually if you need to generate a new migration:

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add <MigrationName> --project backend/GuessNumber.Infrastructure --startup-project backend/GuessNumber.Api
dotnet ef database update --project backend/GuessNumber.Infrastructure --startup-project backend/GuessNumber.Api
```

Note: adjust project paths if you change the solution layout.

## OpenAPI / Swagger
Swagger is enabled in `Program.cs` via `AddEndpointsApiExplorer()` and `AddSwaggerGen()` and the middleware.

If you want JWT input inside Swagger UI (recommended for testing authenticated endpoints) add the following to `AddSwaggerGen(...)` configuration:

```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your token."
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});
```

Then enable middleware (already present) for development:

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

Open `http://localhost:5000/swagger` to view the UI and try endpoints. The `/swagger/v1/swagger.json` endpoint provides the raw OpenAPI document.

### Improving API docs
- Add XML comments to controllers/DTOs and enable `IncludeXmlComments` in Swagger configuration for richer docs.
- Use `Swashbuckle.AspNetCore` filters to customize schema generation.

## API endpoints (quick reference)
Auth (`AuthController`, route `api/auth`):
- `POST /api/auth/register` — Register a user. Body: `RegisterDto`. Response: `AuthResponseDto` with JWT.
- `POST /api/auth/login` — Login. Body: `LoginDto`. Response: `AuthResponseDto` with JWT.
- `POST /api/auth/logout` — Logout. Exists so the frontend can call a consistent API.

Game (`GameController`, route `api/game`, requires JWT unless noted):
- `POST /api/game/start` — Start a new game. Response: `StartGameResponseDto`.
- `POST /api/game/guess` — Submit a guess. Body: `GuessRequestDto` (`{ "guess": 21 }`). Response: `GuessResponseDto`.
- `GET /api/game/leaderboard` — Public leaderboard (`[AllowAnonymous]`).

## Controllers and DTOs
The real controllers live in `backend/GuessNumber.Api/Controllers`:

- `AuthController` — `POST /api/auth/register|login|logout`
- `GameController` — `POST /api/game/start`, `POST /api/game/guess`, `GET /api/game/leaderboard`

All DTOs are in `backend/GuessNumber.Application/DTOs`. The API never returns the
`SecretNumber`; only hints (higher/lower/correct), attempts, and the best score are
exposed to the client.

## DTOs vs Entities
- DTOs (`record` types) are used at the API boundary for concise, immutable data carriers and good schema generation.
- Domain entities (`class`) are used in `GuessNumber.Domain` and are designed to be mutable for EF Core tracking and to encapsulate behavior.

## Security considerations
- Keep `Jwt:Key` and other secrets out of source control and use environment variables or secret stores in production.
- Configure CORS only to trusted origins in production.
- Never return `SecretNumber` in API responses or logs.

## Development workflow
- Use feature branches and pull requests.
- Run unit tests for application and domain logic: `dotnet test` in `backend/GuessNumber.Tests`.
- Run the frontend test suite: `cd frontend && npm test` (Vitest + Testing Library + msw).
- Use the `docker-compose.yml` at the repo root for a full local or cloud deployment — see
  [`DEPLOYMENT.md`](DEPLOYMENT.md) for the Azure VM / HTTPS setup.

## Troubleshooting
- `dotnet restore` failures: check network, NuGet config, and set `DOTNET_CLI_HOME` if needed.
- Build timeouts: run `dotnet build` locally and inspect the full output for errors.
- Port in use: change `ASPNETCORE_URLS` to another port.

## Future improvements
- Add integration tests covering the full API + database flow.
- Add a CI pipeline (build, test, migrations) with GitHub Actions.
- Expand Swagger documentation with JWT authorization support for authenticated endpoints.

