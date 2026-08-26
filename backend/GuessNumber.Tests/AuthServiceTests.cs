using System.Threading.Tasks;
using GuessNumber.Application.DTOs;
using GuessNumber.Application.Interfaces;
using GuessNumber.Application.Services;
using GuessNumber.Domain.Entities;
using NSubstitute;
using Xunit;

namespace GuessNumber.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task Register_CreatesUserAndReturnsToken()
    {
        var users = Substitute.For<IUserRepository>();
        users.ExistsAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(false);
        users.AddAsync(Arg.Any<User>()).Returns(Task.CompletedTask);
        users.SaveChangesAsync().Returns(Task.CompletedTask);

        var hasher = Substitute.For<IPasswordHasher>();
        hasher.Hash(Arg.Any<string>()).Returns("hashed");

        var jwt = Substitute.For<IJwtTokenGenerator>();
        jwt.GenerateToken(Arg.Any<User>()).Returns("token123");

        var svc = new AuthService(users, hasher, jwt);

        var dto = new RegisterDto("u", "u@example.com", "pass123");

        var res = await svc.RegisterAsync(dto);

        Assert.Equal("token123", res.Token);
        Assert.Equal("u", res.Username);
        users.Received(1).AddAsync(Arg.Is<User>(x => x.Username == "u" && x.PasswordHash == "hashed"));
        users.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Register_Existing_Throws()
    {
        var users = Substitute.For<IUserRepository>();
        users.ExistsAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        var hasher = Substitute.For<IPasswordHasher>();
        var jwt = Substitute.For<IJwtTokenGenerator>();

        var svc = new AuthService(users, hasher, jwt);

        var dto = new RegisterDto("u", "u@example.com", "pass123");

        await Assert.ThrowsAsync<GuessNumber.Application.Exceptions.AppException>(() => svc.RegisterAsync(dto));
    }

    [Fact]
    public async Task Login_Success_ReturnsToken()
    {
        var user = new User { Id = 1, Username = "u", PasswordHash = "h" };
        var users = Substitute.For<IUserRepository>();
        users.GetByUsernameAsync("u").Returns(user);

        var hasher = Substitute.For<IPasswordHasher>();
        hasher.Verify("pass", "h").Returns(true);

        var jwt = Substitute.For<IJwtTokenGenerator>();
        jwt.GenerateToken(user).Returns("tkn");

        var svc = new AuthService(users, hasher, jwt);

        var dto = new LoginDto("u", "pass");
        var res = await svc.LoginAsync(dto);

        Assert.Equal("tkn", res.Token);
        Assert.Equal("u", res.Username);
    }

    [Fact]
    public async Task Login_Invalid_Throws()
    {
        var users = Substitute.For<IUserRepository>();
        users.GetByUsernameAsync("u").Returns((User?)null);

        var hasher = Substitute.For<IPasswordHasher>();
        var jwt = Substitute.For<IJwtTokenGenerator>();

        var svc = new AuthService(users, hasher, jwt);

        var dto = new LoginDto("u", "pass");
        await Assert.ThrowsAsync<GuessNumber.Application.Exceptions.AppException>(() => svc.LoginAsync(dto));
    }
}
