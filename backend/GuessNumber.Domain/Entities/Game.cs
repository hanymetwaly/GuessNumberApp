namespace GuessNumber.Domain.Entities;

public class Game
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public int UserId { get; set; }
    public User? User { get; set; }

    // The secret number 1-43, stored server-side so the client can't cheat
    public int SecretNumber { get; set; }

    public int Attempts { get; set; }
    public bool IsFinished { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
