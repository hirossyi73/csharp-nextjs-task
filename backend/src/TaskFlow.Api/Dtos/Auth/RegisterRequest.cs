using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.Dtos.Auth;

/// <summary>
/// ユーザー登録リクエスト DTO
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// メールアドレス
    /// </summary>
    [Required(ErrorMessage = "メールアドレスは必須です")]
    [EmailAddress(ErrorMessage = "メールアドレスの形式が正しくありません")]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// パスワード（8文字以上、大文字・小文字・数字を含む）
    /// </summary>
    [Required(ErrorMessage = "パスワードは必須です")]
    [MinLength(8, ErrorMessage = "パスワードは8文字以上で入力してください")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
        ErrorMessage = "パスワードには大文字・小文字・数字を含めてください")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// パスワード確認
    /// </summary>
    [Required(ErrorMessage = "パスワード確認は必須です")]
    [Compare("Password", ErrorMessage = "パスワードが一致しません")]
    public string PasswordConfirmation { get; set; } = string.Empty;
}
