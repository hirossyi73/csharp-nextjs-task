using TaskFlow.Application.Interfaces;

namespace TaskFlow.Infrastructure.Services;

/// <summary>
/// bcrypt を使用したパスワードハッシュ化サービス
/// </summary>
public class PasswordHasher : PasswordHasherInterface
{
    /// <summary>
    /// パスワードをハッシュ化する
    /// </summary>
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    /// <summary>
    /// パスワードとハッシュを検証する
    /// </summary>
    public bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
