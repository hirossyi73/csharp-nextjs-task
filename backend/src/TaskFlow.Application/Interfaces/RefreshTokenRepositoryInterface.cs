using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces;

/// <summary>
/// リフレッシュトークンリポジトリのインタフェース
/// </summary>
public interface RefreshTokenRepositoryInterface
{
    /// <summary>
    /// トークンハッシュでリフレッシュトークンを検索する
    /// </summary>
    Task<RefreshToken?> FindByTokenHashAsync(string tokenHash);

    /// <summary>
    /// リフレッシュトークンを作成する
    /// </summary>
    Task<RefreshToken> CreateAsync(RefreshToken refreshToken);

    /// <summary>
    /// リフレッシュトークンを更新する
    /// </summary>
    Task UpdateAsync(RefreshToken refreshToken);

    /// <summary>
    /// 指定ユーザーの全リフレッシュトークンを無効化する
    /// </summary>
    Task RevokeAllByUserIdAsync(Guid userId);
}
