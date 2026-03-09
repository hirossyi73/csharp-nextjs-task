using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces;

/// <summary>
/// パスワードリセットトークンリポジトリのインタフェース
/// </summary>
public interface PasswordResetTokenRepositoryInterface
{
    /// <summary>
    /// トークンハッシュでパスワードリセットトークンを検索する
    /// </summary>
    Task<PasswordResetToken?> FindByTokenHashAsync(string tokenHash);

    /// <summary>
    /// パスワードリセットトークンを作成する
    /// </summary>
    Task<PasswordResetToken> CreateAsync(PasswordResetToken token);

    /// <summary>
    /// パスワードリセットトークンを更新する
    /// </summary>
    Task UpdateAsync(PasswordResetToken token);
}
