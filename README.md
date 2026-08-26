# GuessNumberApp

A small .NET 10 web API and domain project for a "guess the number" game.

## Summary
A layered .NET 10 application that demonstrates a clean separation between API, application logic, domain model, and infrastructure. The goals are clarity, testability, and safe handling of secrets (e.g., the secret number is never returned to clients).

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

## Design principles
- Single Responsibility: each layer has a single focus
- Dependency direction: outer layers depend on inner layers; domain has no framework dependencies
- DTOs at the boundary: map domain entities to DTOs before returning to the client
- Avoid leaking sensitive fields (e.g., `SecretNumber`) in API responses

## Build and run
Prerequisites:
- .NET 10 SDK installed

Restore and build the solution:

```bash
dotnet restore
dotnet build GuessNumberApp.slnx
```

Run the API locally (bind to port 5000):

```bash
DOTNET_CLI_HOME=$HOME ASPNETCORE_URLS=http://localhost:5000 dotnet run --project backend/GuessNumber.Api
# then open http://localhost:5000/swagger
```

If you see a `DOTNET_CLI_HOME` warning, ensure the environment variable points to a writable directory.

## Database and Migrations
- Connection strings live in `backend/GuessNumber.Api/appsettings*.json`.
- On startup this project attempts to apply migrations automatically (see `Program.cs`). This is convenient for local development and simple deployments.

Manual migrations (optional):

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate --project backend/GuessNumber.Infrastructure --startup-project backend/GuessNumber.Api
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
- `POST /api/auth/register` — Register a user. Body: `RegisterDto`.
- `POST /api/auth/login` — Login. Body: `LoginDto`. Response: `AuthResponseDto` with JWT.
- `POST /api/games` — Start a new game (authenticated).
- `GET /api/games/{id}` — Get game state (authenticated).
- `POST /api/games/{id}/guess` — Submit a guess. Body: `{ "guess": 21 }`.

## Example controllers (minimal)
Below are compact examples to illustrate typical controller implementations and DTO mapping. These are intentionally minimal — adapt error handling, logging, and dependency injection to your app.

Auth controller (simplified):

```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
  private readonly IAuthService _auth;
  public AuthController(IAuthService auth) => _auth = auth;

  [HttpPost("register")]
  public async Task<IActionResult> Register(RegisterDto dto)
  {
    var result = await _auth.RegisterAsync(dto);
    return CreatedAtAction(null, new { username = result.Username }, result);
  }

  [HttpPost("login")]
  public async Task<IActionResult> Login(LoginDto dto)
  {
    var res = await _auth.LoginAsync(dto);
    if (res is null) return Unauthorized();
    return Ok(res);
  }
}
```

Games controller (simplified):

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GamesController : ControllerBase
{
  private readonly IGameService _games;
  public GamesController(IGameService games) => _games = games;

  [HttpPost]
  public async Task<IActionResult> CreateGame()
  {
    var game = await _games.CreateAsync(User.GetUserId());
    // Return safe DTO that hides secret
    return CreatedAtAction(nameof(Get), new { id = game.Id }, GameDto.FromDomain(game));
  }

  [HttpGet("{id}")]
  public async Task<IActionResult> Get(Guid id)
  {
    var game = await _games.GetAsync(id);
    if (game is null) return NotFound();
    return Ok(GameDto.FromDomain(game));
  }

  [HttpPost("{id}/guess")]
  public async Task<IActionResult> Guess(Guid id, GuessDto dto)
  {
    var result = await _games.SubmitGuessAsync(id, dto.Guess);
    return Ok(result); // could be a status, attempts, hints, etc.
  }
}
```

Example DTOs and mapping helpers:

```csharp
public record GameDto(Guid Id, int Attempts, bool IsFinished, DateTime CreatedAt)
{
  public static GameDto FromDomain(Game g) => new(g.Id, g.Attempts, g.IsFinished, g.CreatedAt);
}

public record GuessDto([Required] int Guess);
```

Notes:
- `GameDto` intentionally omits `SecretNumber` so it is never serialized to the client.
- Use services (`IAuthService`, `IGameService`) to encapsulate business rules; controllers orchestrate and map.
- Add validation attributes to DTOs and return `BadRequest` for invalid model state when needed.

## DTOs vs Entities
- DTOs (`record` types) are used at the API boundary for concise, immutable data carriers and good schema generation.
- Domain entities (`class`) are used in `GuessNumber.Domain` and are designed to be mutable for EF Core tracking and to encapsulate behavior.

## Security considerations
- Keep `Jwt:Key` and other secrets out of source control and use environment variables or secret stores in production.
- Configure CORS only to trusted origins in production.
- Never return `SecretNumber` in API responses or logs.

## Development workflow
- Use feature branches and pull requests.
- Run unit tests for application and domain logic, and use integration tests for DB/EF workflows.
- Consider using Docker or a `devcontainer` to standardize the dev environment.

## Troubleshooting
- `dotnet restore` failures: check network, NuGet config, and set `DOTNET_CLI_HOME` if needed.
- Build timeouts: run `dotnet build` locally and inspect the full output for errors.
- Port in use: change `ASPNETCORE_URLS` to another port.

## Next steps (suggested)
- Add example controllers for `Auth` and `Games` to the repo (I can add them).
- Add integration tests and CI pipeline steps (build, test, run migrations).
- Add Dockerfile and Docker Compose for local dev.

---

If you'd like I can now:
- add concrete controller examples for `Auth` and `Games`, or
- commit Swagger JWT configuration to `Program.cs` and attempt a local build/run.

Tell me which and I'll proceed.
