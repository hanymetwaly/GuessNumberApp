using System.ComponentModel.DataAnnotations;
namespace GuessNumber.Application.DTOs;

public record RegisterDto(
    [Required, MinLength(3)] string Username,
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password);

public record LoginDto(
    [Required] string Username,
    [Required] string Password);

// Returned after successful register/login
public record AuthResponseDto(string Token, string Username, int? BestScore);