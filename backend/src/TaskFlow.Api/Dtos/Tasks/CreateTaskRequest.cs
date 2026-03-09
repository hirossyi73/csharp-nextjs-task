using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.Dtos.Tasks;

/// <summary>
/// タスク作成リクエスト DTO
/// </summary>
public class CreateTaskRequest
{
    /// <summary>
    /// タスクタイトル
    /// </summary>
    [Required(ErrorMessage = "タイトルは必須です")]
    [MaxLength(200, ErrorMessage = "タイトルは200文字以内で入力してください")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// タスクの説明
    /// </summary>
    public string? Description { get; set; }
}
