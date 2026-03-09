using TaskFlow.Application.Common;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Services;

/// <summary>
/// タスク管理のビジネスロジックを提供するサービス
/// </summary>
public class TaskService : TaskServiceInterface
{
    private readonly TaskRepositoryInterface _taskRepository;

    /// <summary>
    /// TaskService を生成する
    /// </summary>
    public TaskService(TaskRepositoryInterface taskRepository)
    {
        _taskRepository = taskRepository;
    }

    /// <summary>
    /// ユーザーのタスク一覧を取得する
    /// </summary>
    public async Task<Result<TaskListResult>> GetTasksAsync(Guid userId, int page, int perPage, string? status)
    {
        Domain.Enums.TaskStatus? statusFilter = null;

        if (!string.IsNullOrEmpty(status))
        {
            statusFilter = parseTaskStatus(status);
            if (statusFilter == null)
            {
                return Result<TaskListResult>.Failure(
                    new AppError("TASK_INVALID_STATUS", "無効なステータスです。NOT_STARTED, IN_PROGRESS, COMPLETED のいずれかを指定してください"));
            }
        }

        var (items, totalCount) = await _taskRepository.FindByUserIdAsync(userId, page, perPage, statusFilter);

        var taskResults = items.Select(toTaskResult).ToList();
        var totalPages = (int)Math.Ceiling((double)totalCount / perPage);

        return Result<TaskListResult>.Success(new TaskListResult(
            taskResults,
            new PaginationResult(page, perPage, totalCount, totalPages)));
    }

    /// <summary>
    /// タスクの詳細を取得する
    /// </summary>
    public async Task<Result<TaskResult>> GetTaskByIdAsync(Guid userId, Guid taskId)
    {
        var task = await _taskRepository.FindByIdAsync(taskId);

        if (task == null || task.UserId != userId)
        {
            return Result<TaskResult>.Failure(
                new AppError("TASK_NOT_FOUND", "タスクが見つかりません"));
        }

        return Result<TaskResult>.Success(toTaskResult(task));
    }

    /// <summary>
    /// タスクを作成する
    /// </summary>
    public async Task<Result<TaskResult>> CreateTaskAsync(Guid userId, string title, string? description)
    {
        var task = new TaskItem
        {
            UserId = userId,
            Title = title,
            Description = description,
            Status = Domain.Enums.TaskStatus.NotStarted,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _taskRepository.CreateAsync(task);

        return Result<TaskResult>.Success(toTaskResult(task));
    }

    /// <summary>
    /// タスクを更新する
    /// </summary>
    public async Task<Result<TaskResult>> UpdateTaskAsync(Guid userId, Guid taskId, string title, string? description, string status)
    {
        var task = await _taskRepository.FindByIdAsync(taskId);

        if (task == null || task.UserId != userId)
        {
            return Result<TaskResult>.Failure(
                new AppError("TASK_NOT_FOUND", "タスクが見つかりません"));
        }

        var parsedStatus = parseTaskStatus(status);
        if (parsedStatus == null)
        {
            return Result<TaskResult>.Failure(
                new AppError("TASK_INVALID_STATUS", "無効なステータスです。NOT_STARTED, IN_PROGRESS, COMPLETED のいずれかを指定してください"));
        }

        task.Title = title;
        task.Description = description;
        task.Status = parsedStatus.Value;
        task.UpdatedAt = DateTime.UtcNow;

        await _taskRepository.UpdateAsync(task);

        return Result<TaskResult>.Success(toTaskResult(task));
    }

    /// <summary>
    /// タスクを削除する
    /// </summary>
    public async Task<Result> DeleteTaskAsync(Guid userId, Guid taskId)
    {
        var task = await _taskRepository.FindByIdAsync(taskId);

        if (task == null || task.UserId != userId)
        {
            return Result.Failure(
                new AppError("TASK_NOT_FOUND", "タスクが見つかりません"));
        }

        await _taskRepository.DeleteAsync(task);

        return Result.Success();
    }

    /// <summary>
    /// UPPER_SNAKE_CASE 文字列を TaskStatus 列挙型に変換する
    /// </summary>
    private static Domain.Enums.TaskStatus? parseTaskStatus(string status)
    {
        return status switch
        {
            "NOT_STARTED" => Domain.Enums.TaskStatus.NotStarted,
            "IN_PROGRESS" => Domain.Enums.TaskStatus.InProgress,
            "COMPLETED" => Domain.Enums.TaskStatus.Completed,
            _ => null
        };
    }

    /// <summary>
    /// TaskStatus 列挙型を UPPER_SNAKE_CASE 文字列に変換する
    /// </summary>
    private static string formatTaskStatus(Domain.Enums.TaskStatus status)
    {
        return status switch
        {
            Domain.Enums.TaskStatus.NotStarted => "NOT_STARTED",
            Domain.Enums.TaskStatus.InProgress => "IN_PROGRESS",
            Domain.Enums.TaskStatus.Completed => "COMPLETED",
            _ => "NOT_STARTED"
        };
    }

    /// <summary>
    /// TaskItem エンティティを TaskResult に変換する
    /// </summary>
    private static TaskResult toTaskResult(TaskItem task)
    {
        return new TaskResult(
            task.Id,
            task.Title,
            task.Description,
            formatTaskStatus(task.Status),
            task.CreatedAt,
            task.UpdatedAt);
    }
}

/// <summary>
/// タスク情報の結果を表す
/// </summary>
public record TaskResult(Guid Id, string Title, string? Description, string Status, DateTime CreatedAt, DateTime UpdatedAt);

/// <summary>
/// タスク一覧の結果を表す
/// </summary>
public record TaskListResult(List<TaskResult> Items, PaginationResult Pagination);

/// <summary>
/// ページネーション情報を表す
/// </summary>
public record PaginationResult(int CurrentPage, int PerPage, int TotalCount, int TotalPages);
