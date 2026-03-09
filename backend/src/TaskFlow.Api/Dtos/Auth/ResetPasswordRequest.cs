using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.Dtos.Auth;

/// <summary>
/// パスワードリセット実行 DTO
/// </summary>
public class ResetPasswordRequest
{
    /// <summary>
    /// パスワードリセットトークン
    /// </summary>
    [Required(ErrorMessage = "トークンは必須です")]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// 新しいパスワード（8文字以上、大文字・小文字・数字を含む）
    /// </summary>
    [Required(ErrorMessage = "新しいパスワードは必須です")]
    [MinLength(8, ErrorMessage = "パスワードは8文字以上で入力してください")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
        ErrorMessage = "パスワードには大文字・小文字・数字を含めてください")]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// 新しいパスワード確認
    /// </summary>
    [Required(ErrorMessage = "パスワード確認は必須です")]
    [Compare("NewPassword", ErrorMessage = "パスワードが一致しません")]
    public string NewPasswordConfirmation { get; set; } = string.Empty;
}
