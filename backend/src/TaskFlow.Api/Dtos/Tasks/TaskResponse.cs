namespace TaskFlow.Api.Dtos.Tasks;

/// <summary>
/// タスクレスポンス DTO
/// </summary>
public class TaskResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// タスク一覧レスポンス DTO
/// </summary>
public class TaskListResponse
{
    public List<TaskResponse> Items { get; set; } = new();
    public PaginationResponse Pagination { get; set; } = new();
}

/// <summary>
/// ページネーションレスポンス DTO
/// </summary>
public class PaginationResponse
{
    public int CurrentPage { get; set; }
    public int PerPage { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}
