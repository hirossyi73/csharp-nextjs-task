using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces;

/// <summary>
/// タスクリポジトリのインタフェース
/// </summary>
public interface TaskRepositoryInterface
{
    /// <summary>
    /// ユーザーのタスク一覧を取得する
    /// </summary>
    Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> FindByUserIdAsync(
        Guid userId, int page, int perPage, Domain.Enums.TaskStatus? status = null);

    /// <summary>
    /// ID でタスクを検索する
    /// </summary>
    Task<TaskItem?> FindByIdAsync(Guid id);

    /// <summary>
    /// タスクを作成する
    /// </summary>
    Task<TaskItem> CreateAsync(TaskItem task);

    /// <summary>
    /// タスクを更新する
    /// </summary>
    Task UpdateAsync(TaskItem task);

    /// <summary>
    /// タスクを削除する
    /// </summary>
    Task DeleteAsync(TaskItem task);
}
