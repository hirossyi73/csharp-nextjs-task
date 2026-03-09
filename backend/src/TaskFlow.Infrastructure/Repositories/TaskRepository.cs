using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Infrastructure.Repositories;

/// <summary>
/// タスクリポジトリの EF Core 実装
/// </summary>
public class TaskRepository : TaskRepositoryInterface
{
    private readonly AppDbContext _context;

    /// <summary>
    /// TaskRepository を生成する
    /// </summary>
    public TaskRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// ユーザーのタスク一覧を取得する
    /// </summary>
    public async Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> FindByUserIdAsync(
        Guid userId, int page, int perPage, Domain.Enums.TaskStatus? status = null)
    {
        var query = _context.Tasks
            .Where(t => t.UserId == userId);

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync();

        return (items, totalCount);
    }

    /// <summary>
    /// ID でタスクを検索する
    /// </summary>
    public async Task<TaskItem?> FindByIdAsync(Guid id)
    {
        return await _context.Tasks.FindAsync(id);
    }

    /// <summary>
    /// タスクを作成する
    /// </summary>
    public async Task<TaskItem> CreateAsync(TaskItem task)
    {
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    /// <summary>
    /// タスクを更新する
    /// </summary>
    public async Task UpdateAsync(TaskItem task)
    {
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// タスクを削除する
    /// </summary>
    public async Task DeleteAsync(TaskItem task)
    {
        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
    }
}
