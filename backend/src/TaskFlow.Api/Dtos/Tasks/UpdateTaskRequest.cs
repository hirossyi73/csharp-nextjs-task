using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.Dtos.Tasks;

/// <summary>
/// タスク更新リクエスト DTO
/// </summary>
public class UpdateTaskRequest
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

    /// <summary>
    /// ステータス（NOT_STARTED, IN_PROGRESS, COMPLETED）
    /// </summary>
    [Required(ErrorMessage = "ステータスは必須です")]
    public string Status { get; set; } = string.Empty;
}
