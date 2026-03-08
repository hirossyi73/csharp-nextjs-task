namespace TaskFlow.Domain.Entities;

/// <summary>
/// JWT リフレッシュトークンエンティティ。トークンローテーションと無効化を管理する
/// </summary>
public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
}
