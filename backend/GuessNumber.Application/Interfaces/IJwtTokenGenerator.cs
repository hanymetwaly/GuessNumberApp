using GuessNumber.Domain.Entities;
namespace GuessNumber.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}