using GuessNumber.Application.Interfaces;
using GuessNumber.Domain.Entities;
using GuessNumber.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GuessNumber.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;
    public UserRepository(AppDbContext db) => _db = db;

    public Task<User?> GetByUsernameAsync(string username) =>
        _db.Users.FirstOrDefaultAsync(u => u.Username == username);

    public Task<User?> GetByIdAsync(int id) =>
        _db.Users.FirstOrDefaultAsync(u => u.Id == id);

    public Task<bool> ExistsAsync(string username, string email) =>
        _db.Users.AnyAsync(u => u.Username == username || u.Email == email);

    public async Task AddAsync(User user) => await _db.Users.AddAsync(user);

    public async Task<List<User>> GetTopScorersAsync(int count) =>
        await _db.Users
            .Where(u => u.BestScore != null)
            .OrderBy(u => u.BestScore)
            .ThenBy(u => u.CreatedAt)
            .Take(count)
            .ToListAsync();

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}