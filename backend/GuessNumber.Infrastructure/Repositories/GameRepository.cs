using GuessNumber.Application.Interfaces;
using GuessNumber.Domain.Entities;
using GuessNumber.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GuessNumber.Infrastructure.Repositories;

public class GameRepository : IGameRepository
{
    private readonly AppDbContext _db;
    public GameRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(Game game) => await _db.Games.AddAsync(game);

    public Task<Game?> GetByIdAsync(Guid id) =>
        _db.Games.FirstOrDefaultAsync(g => g.Id == id);

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}