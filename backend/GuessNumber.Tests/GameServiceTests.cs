using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GuessNumber.Application.DTOs;
using GuessNumber.Application.Services;
using GuessNumber.Domain.Entities;
using NSubstitute;
using Xunit;

namespace GuessNumber.Tests;

public class GameServiceTests
{
    [Fact]
    public async Task StartGame_CreatesGameAndReturnsMessage()
    {
        var games = Substitute.For<GuessNumber.Application.Interfaces.IGameRepository>();
        games.AddAsync(Arg.Any<Game>()).Returns(Task.CompletedTask);
        games.SaveChangesAsync().Returns(Task.CompletedTask);

        var users = Substitute.For<GuessNumber.Application.Interfaces.IUserRepository>();

        var svc = new GameService(games, users);

        var res = await svc.StartGameAsync(42);

        Assert.False(res.GameId == Guid.Empty);
        Assert.Contains("thinking of a number", res.Message, StringComparison.OrdinalIgnoreCase);
        games.Received(1).AddAsync(Arg.Is<Game>(x => x.UserId == 42 && x.Attempts == 0));
    }

    [Fact]
    public async Task Guess_WrongHigher_ReturnsHigher()
    {
        var secret = 30;
        var game = new Game { Id = Guid.NewGuid(), UserId = 1, SecretNumber = secret, Attempts = 0, IsFinished = false };

        var games = Substitute.For<GuessNumber.Application.Interfaces.IGameRepository>();
        games.GetByIdAsync(game.Id).Returns(game);
        games.SaveChangesAsync().Returns(Task.CompletedTask);

        var users = Substitute.For<GuessNumber.Application.Interfaces.IUserRepository>();

        var svc = new GameService(games, users);

        var dto = new GuessRequestDto(game.Id, 20);
        var res = await svc.GuessAsync(1, dto);

        Assert.Equal("higher", res.Result);
        Assert.False(res.Finished);
        Assert.Equal(1, res.Attempts);
    }

    [Fact]
    public async Task Guess_Correct_UpdatesUserBestScore()
    {
        var game = new Game { Id = Guid.NewGuid(), UserId = 2, SecretNumber = 10, Attempts = 2, IsFinished = false };
        var user = new User { Id = 2, Username = "bob", BestScore = 5 };

        var games = Substitute.For<GuessNumber.Application.Interfaces.IGameRepository>();
        games.GetByIdAsync(game.Id).Returns(game);
        games.SaveChangesAsync().Returns(Task.CompletedTask);

        var users = Substitute.For<GuessNumber.Application.Interfaces.IUserRepository>();
        users.GetByIdAsync(2).Returns(user);
        users.SaveChangesAsync().Returns(Task.CompletedTask);

        var svc = new GameService(games, users);

        var dto = new GuessRequestDto(game.Id, 10);
        var res = await svc.GuessAsync(2, dto);

        Assert.Equal("correct", res.Result);
        Assert.True(res.Finished);
        Assert.True(res.IsNewRecord);
        Assert.Equal(user.BestScore, res.BestScore);
    }

    [Fact]
    public async Task GetLeaderboard_ReturnsTopScorers()
    {
        var users = new List<User>
        {
            new User { Id = 1, Username = "a", BestScore = 2 },
            new User { Id = 2, Username = "b", BestScore = 3 }
        };

        var usersRepo = Substitute.For<GuessNumber.Application.Interfaces.IUserRepository>();
        usersRepo.GetTopScorersAsync(10).Returns(users);

        var gamesRepo = Substitute.For<GuessNumber.Application.Interfaces.IGameRepository>();

        var svc = new GameService(gamesRepo, usersRepo);

        var res = await svc.GetLeaderboardAsync(10);

        Assert.Equal(2, res.Count);
        Assert.Equal("a", res[0].Username);
        Assert.Equal(1, res[0].Rank);
    }
}
