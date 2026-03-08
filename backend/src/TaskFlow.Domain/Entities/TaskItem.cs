namespace TaskFlow.Domain.Entities;

/// <summary>
/// タスクエンティティ。System.Threading.Tasks.Task との名前衝突を避けるため TaskItem とする
/// </summary>
public class TaskItem
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Enums.TaskStatus Status { get; set; } = Enums.TaskStatus.NotStarted;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
}
