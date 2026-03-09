using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Dtos.Tasks;
using TaskFlow.Application.Common;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Services;

namespace TaskFlow.Api.Controllers;

/// <summary>
/// タスク管理のエンドポイントを提供するコントローラー
/// </summary>
[ApiController]
[Route("api/v1/tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly TaskServiceInterface _taskService;

    /// <summary>
    /// TasksController を生成する
    /// </summary>
    public TasksController(TaskServiceInterface taskService)
    {
        _taskService = taskService;
    }

    /// <summary>
    /// タスク一覧取得
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTasks(
        [FromQuery] int page = 1,
        [FromQuery] int perPage = 20,
        [FromQuery] string? status = null)
    {
        var userId = getUserIdFromClaims();
        if (userId == null) return Unauthorized();

        if (perPage > 100) perPage = 100;
        if (page < 1) page = 1;

        var result = await _taskService.GetTasksAsync(userId.Value, page, perPage, status);

        if (result.IsFailure)
        {
            return toErrorResponse(result.Error);
        }

        var value = result.Value;
        return Ok(new TaskListResponse
        {
            Items = value.Items.Select(toResponse).ToList(),
            Pagination = new PaginationResponse
            {
                CurrentPage = value.Pagination.CurrentPage,
                PerPage = value.Pagination.PerPage,
                TotalCount = value.Pagination.TotalCount,
                TotalPages = value.Pagination.TotalPages
            }
        });
    }

    /// <summary>
    /// タスク詳細取得
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTask(Guid id)
    {
        var userId = getUserIdFromClaims();
        if (userId == null) return Unauthorized();

        var result = await _taskService.GetTaskByIdAsync(userId.Value, id);

        if (result.IsFailure)
        {
            return toErrorResponse(result.Error);
        }

        return Ok(toResponse(result.Value));
    }

    /// <summary>
    /// タスク作成
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request)
    {
        var userId = getUserIdFromClaims();
        if (userId == null) return Unauthorized();

        var result = await _taskService.CreateTaskAsync(userId.Value, request.Title, request.Description);

        if (result.IsFailure)
        {
            return toErrorResponse(result.Error);
        }

        var response = toResponse(result.Value);
        return CreatedAtAction(nameof(GetTask), new { id = response.Id }, response);
    }

    /// <summary>
    /// タスク更新
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateTask(Guid id, [FromBody] UpdateTaskRequest request)
    {
        var userId = getUserIdFromClaims();
        if (userId == null) return Unauthorized();

        var result = await _taskService.UpdateTaskAsync(userId.Value, id, request.Title, request.Description, request.Status);

        if (result.IsFailure)
        {
            return toErrorResponse(result.Error);
        }

        return Ok(toResponse(result.Value));
    }

    /// <summary>
    /// タスク削除
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTask(Guid id)
    {
        var userId = getUserIdFromClaims();
        if (userId == null) return Unauthorized();

        var result = await _taskService.DeleteTaskAsync(userId.Value, id);

        if (result.IsFailure)
        {
            return toErrorResponse(result.Error);
        }

        return NoContent();
    }

    /// <summary>
    /// TaskResult を TaskResponse に変換する
    /// </summary>
    private static TaskResponse toResponse(TaskResult task)
    {
        return new TaskResponse
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };
    }

    /// <summary>
    /// AppError を HTTP エラーレスポンスに変換する
    /// </summary>
    private IActionResult toErrorResponse(AppError error)
    {
        var statusCode = error.Code switch
        {
            "TASK_NOT_FOUND" => StatusCodes.Status404NotFound,
            "TASK_INVALID_STATUS" => StatusCodes.Status422UnprocessableEntity,
            "VALIDATION_ERROR" => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status400BadRequest
        };

        return StatusCode(statusCode, new { error = new { code = error.Code, message = error.Message } });
    }

    /// <summary>
    /// JWT クレームからユーザー ID を取得する
    /// </summary>
    private Guid? getUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim != null && Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return null;
    }
}
