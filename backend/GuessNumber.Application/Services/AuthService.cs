using GuessNumber.Application.DTOs;
using GuessNumber.Application.Exceptions;
using GuessNumber.Application.Interfaces;
using GuessNumber.Domain.Entities;

namespace GuessNumber.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenGenerator _jwt;

    public AuthService(IUserRepository users, IPasswordHasher hasher, IJwtTokenGenerator jwt)
    {
        _users = users;
        _hasher = hasher;
        _jwt = jwt;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        if (await _users.ExistsAsync(dto.Username, dto.Email))
            throw new AppException("Username or email is already taken.");

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = _hasher.Hash(dto.Password)
        };

        await _users.AddAsync(user);
        await _users.SaveChangesAsync();

        var token = _jwt.GenerateToken(user);
        return new AuthResponseDto(token, user.Username, user.BestScore);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _users.GetByUsernameAsync(dto.Username);
        if (user is null || !_hasher.Verify(dto.Password, user.PasswordHash))
            throw new AppException("Invalid username or password.");

        var token = _jwt.GenerateToken(user);
        return new AuthResponseDto(token, user.Username, user.BestScore);
    }
}