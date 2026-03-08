namespace TaskFlow.Domain.Enums;

/// <summary>
/// タスクの進捗ステータスを表す列挙型。DB には SMALLINT として保存される
/// </summary>
public enum TaskStatus
{
    NotStarted = 0,
    InProgress = 1,
    Completed = 2
}
