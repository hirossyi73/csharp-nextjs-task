using TaskFlow.Application.Common;
using TaskFlow.Application.Services;

namespace TaskFlow.Application.Interfaces;

/// <summary>
/// タスクサービスのインタフェース
/// </summary>
public interface TaskServiceInterface
{
    /// <summary>
    /// ユーザーのタスク一覧を取得する
    /// </summary>
    Task<Result<TaskListResult>> GetTasksAsync(Guid userId, int page, int perPage, string? status);

    /// <summary>
    /// タスクの詳細を取得する
    /// </summary>
    Task<Result<TaskResult>> GetTaskByIdAsync(Guid userId, Guid taskId);

    /// <summary>
    /// タスクを作成する
    /// </summary>
    Task<Result<TaskResult>> CreateTaskAsync(Guid userId, string title, string? description);

    /// <summary>
    /// タスクを更新する
    /// </summary>
    Task<Result<TaskResult>> UpdateTaskAsync(Guid userId, Guid taskId, string title, string? description, string status);

    /// <summary>
    /// タスクを削除する
    /// </summary>
    Task<Result> DeleteTaskAsync(Guid userId, Guid taskId);
}
