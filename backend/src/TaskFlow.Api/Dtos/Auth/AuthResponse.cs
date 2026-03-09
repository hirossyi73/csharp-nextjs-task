namespace TaskFlow.Api.Dtos.Auth;

/// <summary>
/// ログイン・トークンリフレッシュ成功時のレスポンス DTO
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// アクセストークン
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// アクセストークンの有効期限（秒）
    /// </summary>
    public int ExpiresIn { get; set; }
}

/// <summary>
/// メッセージのみのレスポンス DTO
/// </summary>
public class MessageResponse
{
    /// <summary>
    /// メッセージ
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
