using TaskFlow.Application.Common;
using TaskFlow.Application.Services;

namespace TaskFlow.Application.Interfaces;

/// <summary>
/// 認証サービスのインタフェース
/// </summary>
public interface AuthServiceInterface
{
    /// <summary>
    /// ユーザーを仮登録し、確認メールを送信する
    /// </summary>
    Task<Result> RegisterAsync(string email, string password);

    /// <summary>
    /// メール確認トークンを検証し、ユーザーを本登録する
    /// </summary>
    Task<Result> VerifyEmailAsync(string token);

    /// <summary>
    /// ログイン認証を行い、アクセストークンとリフレッシュトークンを返す
    /// </summary>
    Task<Result<LoginResult>> LoginAsync(string email, string password);

    /// <summary>
    /// ログアウト処理を行い、ユーザーの全リフレッシュトークンを無効化する
    /// </summary>
    Task<Result> LogoutAsync(Guid userId);

    /// <summary>
    /// リフレッシュトークンを検証し、新しいアクセストークンとリフレッシュトークンを発行する
    /// </summary>
    Task<Result<LoginResult>> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// パスワードリセット要求を処理し、リセットメールを送信する
    /// </summary>
    Task<Result> ForgotPasswordAsync(string email);

    /// <summary>
    /// パスワードリセットトークンを検証し、パスワードを再設定する
    /// </summary>
    Task<Result> ResetPasswordAsync(string token, string newPassword);
}
