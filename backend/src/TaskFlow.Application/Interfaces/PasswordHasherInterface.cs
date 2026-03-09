namespace TaskFlow.Application.Interfaces;

/// <summary>
/// パスワードハッシュ化のインタフェース
/// </summary>
public interface PasswordHasherInterface
{
    /// <summary>
    /// パスワードをハッシュ化する
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// パスワードとハッシュを検証する
    /// </summary>
    bool VerifyPassword(string password, string passwordHash);
}
