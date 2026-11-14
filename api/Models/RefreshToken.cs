namespace api.Models;

public class RefreshToken
{
    public int Id { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public string AppUserId { get; set; } = string.Empty;
    public AppUser? AppUser { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    // set when the token is rotated; rotated tokens are kept until they expire to detect reuse
    public DateTime? RevokedAt { get; set; }
}
