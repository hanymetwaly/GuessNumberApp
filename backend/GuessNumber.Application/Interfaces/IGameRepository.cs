using GuessNumber.Domain.Entities;
namespace GuessNumber.Application.Interfaces;

public interface IGameRepository
{
    Task AddAsync(Game game);
    Task<Game?> GetByIdAsync(Guid id);
    Task SaveChangesAsync();
}
