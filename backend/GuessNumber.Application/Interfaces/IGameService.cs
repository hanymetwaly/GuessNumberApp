using GuessNumber.Application.DTOs;
namespace GuessNumber.Application.Interfaces;

public interface IGameService
{
    Task<StartGameResponseDto> StartGameAsync(int userId);
    Task<GuessResponseDto> GuessAsync(int userId, GuessRequestDto dto);
    Task<List<LeaderboardEntryDto>> GetLeaderboardAsync(int count);
}