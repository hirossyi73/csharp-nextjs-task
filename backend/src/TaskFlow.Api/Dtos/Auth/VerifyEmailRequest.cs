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
}
