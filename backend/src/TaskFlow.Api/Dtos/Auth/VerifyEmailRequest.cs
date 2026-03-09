using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.Dtos.Auth;

/// <summary>
/// メール確認リクエスト DTO
/// </summary>
public class VerifyEmailRequest
{
    /// <summary>
    /// メール確認トークン
    /// </summary>
    [Required(ErrorMessage = "トークンは必須です")]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// パスワード（8文字以上、大文字・小文字・数字を含む）
    /// </summary>
    [MinLength(8, ErrorMessage = "パスワードは8文字以上で入力してください")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
        ErrorMessage = "パスワードには大文字・小文字・数字を含めてください")]
    public string? Password { get; set; }

    /// <summary>
    /// パスワード確認
    /// </summary>
    [Compare("Password", ErrorMessage = "パスワードが一致しません")]
    public string? PasswordConfirmation { get; set; }
}
