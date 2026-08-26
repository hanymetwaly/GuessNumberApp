using System.ComponentModel.DataAnnotations;
namespace GuessNumber.Application.DTOs;

public record StartGameResponseDto(Guid GameId, string Message);
 
public record GuessRequestDto(
    [Required] Guid GameId,
    [Range(1, 43)] int Guess);
 
public record GuessResponseDto(
    string Result,        // "higher" | "lower" | "correct"
    int Attempts,
    bool Finished,
    bool IsNewRecord,
    int? BestScore);
 
public record LeaderboardEntryDto(int Rank, string Username, int BestScore);