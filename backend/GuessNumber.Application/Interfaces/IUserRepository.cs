using GuessNumber.Domain.Entities;
namespace GuessNumber.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByIdAsync(int id);
    Task<bool> ExistsAsync(string username, string email);
    Task AddAsync(User user);
    Task<List<User>> GetTopScorersAsync(int count);
    Task SaveChangesAsync();
}