using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.Dtos.Auth;

/// <summary>
/// ログインリクエスト DTO
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// メールアドレス
    /// </summary>
    [Required(ErrorMessage = "メールアドレスは必須です")]
    [EmailAddress(ErrorMessage = "メールアドレスの形式が正しくありません")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// パスワード
    /// </summary>
    [Required(ErrorMessage = "パスワードは必須です")]
    public string Password { get; set; } = string.Empty;
}
