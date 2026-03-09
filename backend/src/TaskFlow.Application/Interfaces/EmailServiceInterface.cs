namespace TaskFlow.Application.Interfaces;

/// <summary>
/// メール送信のインタフェース
/// </summary>
public interface EmailServiceInterface
{
    /// <summary>
    /// メール確認メールを送信する
    /// </summary>
    Task SendEmailVerificationAsync(string toEmail, string token);

    /// <summary>
    /// パスワードリセットメールを送信する
    /// </summary>
    Task SendPasswordResetAsync(string toEmail, string token);
}
