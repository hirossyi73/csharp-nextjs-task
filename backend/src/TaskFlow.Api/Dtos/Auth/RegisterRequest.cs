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
}
