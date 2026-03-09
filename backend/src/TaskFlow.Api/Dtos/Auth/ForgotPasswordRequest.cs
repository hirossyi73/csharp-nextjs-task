using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.Dtos.Auth;

/// <summary>
/// パスワードリセット要求 DTO
/// </summary>
public class ForgotPasswordRequest
{
    /// <summary>
    /// メールアドレス
    /// </summary>
    [Required(ErrorMessage = "メールアドレスは必須です")]
    [EmailAddress(ErrorMessage = "メールアドレスの形式が正しくありません")]
    public string Email { get; set; } = string.Empty;
}
