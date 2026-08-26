using GuessNumber.Api.Extensions;
using GuessNumber.Application.DTOs;
using GuessNumber.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GuessNumber.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // all game endpoints require a valid JWT
public class GameController : ControllerBase
{
    private readonly IGameService _game;
    public GameController(IGameService game) => _game = game;

    [HttpPost("start")]
    public async Task<ActionResult<StartGameResponseDto>> Start()
        => Ok(await _game.StartGameAsync(User.GetUserId()));

    [HttpPost("guess")]
    public async Task<ActionResult<GuessResponseDto>> Guess(GuessRequestDto dto)
        => Ok(await _game.GuessAsync(User.GetUserId(), dto));

    [HttpGet("leaderboard")]
    [AllowAnonymous] // leaderboard is public
    public async Task<ActionResult<List<LeaderboardEntryDto>>> Leaderboard([FromQuery] int count = 10)
        => Ok(await _game.GetLeaderboardAsync(count));
}