using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces;

/// <summary>
/// メール確認トークンリポジトリのインタフェース
/// </summary>
public interface EmailVerificationTokenRepositoryInterface
{
    /// <summary>
    /// トークンハッシュでメール確認トークンを検索する
    /// </summary>
    Task<EmailVerificationToken?> FindByTokenHashAsync(string tokenHash);

    /// <summary>
    /// メール確認トークンを作成する
    /// </summary>
    Task<EmailVerificationToken> CreateAsync(EmailVerificationToken token);

    /// <summary>
    /// メール確認トークンを更新する
    /// </summary>
    Task UpdateAsync(EmailVerificationToken token);

    /// <summary>
    /// 指定ユーザーの未使用トークンを全て無効化する
    /// </summary>
    Task InvalidateByUserIdAsync(Guid userId);
}
