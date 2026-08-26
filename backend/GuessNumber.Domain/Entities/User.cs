namespace GuessNumber.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    // The "lowest number of guesses" record. Null = never won yet.
    public int? BestScore { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    //a user has many games
    public ICollection<Game> Games { get; set; } = new List<Game>();
}
