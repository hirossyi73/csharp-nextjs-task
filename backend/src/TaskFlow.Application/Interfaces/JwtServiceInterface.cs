namespace TaskFlow.Application.Interfaces;

/// <summary>
/// JWT トークン生成・検証のインタフェース
/// </summary>
public interface JwtServiceInterface
{
    /// <summary>
    /// アクセストークンを生成する
    /// </summary>
    string GenerateAccessToken(Guid userId, string email);

    /// <summary>
    /// リフレッシュトークンを生成する
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// アクセストークンからユーザー ID を取得する
    /// </summary>
    Guid? GetUserIdFromToken(string token);

    /// <summary>
    /// アクセストークンの有効期限（秒）を取得する
    /// </summary>
    int GetAccessTokenExpirationSeconds();
}
