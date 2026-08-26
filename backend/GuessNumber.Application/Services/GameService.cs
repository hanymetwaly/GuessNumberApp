using GuessNumber.Application.DTOs;
using GuessNumber.Application.Exceptions;
using GuessNumber.Application.Interfaces;
using GuessNumber.Domain.Entities;

namespace GuessNumber.Application.Services;

public class GameService : IGameService
{
    private readonly IGameRepository _games;
    private readonly IUserRepository _users;

    public GameService(IGameRepository games, IUserRepository users)
    {
        _games = games;
        _users = users;
    }

    public async Task<StartGameResponseDto> StartGameAsync(int userId)
    {
        var secret = Random.Shared.Next(1, 44); // 1..43 inclusive

        var game = new Game
        {
            UserId = userId,
            SecretNumber = secret,
            Attempts = 0,
            IsFinished = false
        };

        await _games.AddAsync(game);
        await _games.SaveChangesAsync();

        return new StartGameResponseDto(game.Id, "I'm thinking of a number between 1 and 43. Take a guess!");
    }

    public async Task<GuessResponseDto> GuessAsync(int userId, GuessRequestDto dto)
    {
        var game = await _games.GetByIdAsync(dto.GameId);

        if (game is null || game.UserId != userId)
            throw new AppException("Game not found.");
        if (game.IsFinished)
            throw new AppException("This game is already finished. Start a new one.");

        game.Attempts++;

        // Wrong guess -> tell them higher or lower
        if (dto.Guess < game.SecretNumber)
        {
            await _games.SaveChangesAsync();
            return new GuessResponseDto("higher", game.Attempts, false, false, null);
        }
        if (dto.Guess > game.SecretNumber)
        {
            await _games.SaveChangesAsync();
            return new GuessResponseDto("lower", game.Attempts, false, false, null);
        }

        // Correct!
        game.IsFinished = true;

        var user = await _users.GetByIdAsync(userId)
                   ?? throw new AppException("User not found.");

        bool isNewRecord = user.BestScore is null || game.Attempts < user.BestScore;
        if (isNewRecord)
            user.BestScore = game.Attempts;

        await _games.SaveChangesAsync();      // saves game (and tracked user in same context)
        await _users.SaveChangesAsync();

        return new GuessResponseDto("correct", game.Attempts, true, isNewRecord, user.BestScore);
    }

    public async Task<List<LeaderboardEntryDto>> GetLeaderboardAsync(int count)
    {
        var top = await _users.GetTopScorersAsync(count);
        return top
            .Select((u, i) => new LeaderboardEntryDto(i + 1, u.Username, u.BestScore!.Value))
            .ToList();
    }
}