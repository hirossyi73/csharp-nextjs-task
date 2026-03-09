using FluentAssertions;
using Moq;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Services;
using TaskFlow.Domain.Entities;
using Xunit;

namespace TaskFlow.Tests.Unit.Services;

/// <summary>
/// TaskService の単体テスト
/// </summary>
public class TaskServiceTests
{
    private readonly Mock<TaskRepositoryInterface> _taskRepository;
    private readonly TaskService _sut;

    /// <summary>
    /// テストの共通セットアップ
    /// </summary>
    public TaskServiceTests()
    {
        _taskRepository = new Mock<TaskRepositoryInterface>();
        _sut = new TaskService(_taskRepository.Object);
    }

    // ========== CreateTaskAsync ==========

    [Fact]
    /// <summary>
    /// 有効な入力でタスクが作成されることを検証する
    /// </summary>
    public async Task CreateTaskAsync_有効な入力_タスクが作成される()
    {
        var userId = Guid.NewGuid();
        _taskRepository.Setup(r => r.CreateAsync(It.IsAny<TaskItem>()))
            .ReturnsAsync((TaskItem t) => t);

        var result = await _sut.CreateTaskAsync(userId, "テストタスク", "説明文");

        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("テストタスク");
        result.Value.Description.Should().Be("説明文");
        result.Value.Status.Should().Be("NOT_STARTED");
        _taskRepository.Verify(r => r.CreateAsync(It.Is<TaskItem>(t =>
            t.UserId == userId &&
            t.Title == "テストタスク" &&
            t.Status == Domain.Enums.TaskStatus.NotStarted)), Times.Once);
    }

    [Fact]
    /// <summary>
    /// 説明なしでタスクが作成できることを検証する
    /// </summary>
    public async Task CreateTaskAsync_説明なし_タスクが作成される()
    {
        var userId = Guid.NewGuid();
        _taskRepository.Setup(r => r.CreateAsync(It.IsAny<TaskItem>()))
            .ReturnsAsync((TaskItem t) => t);

        var result = await _sut.CreateTaskAsync(userId, "タスク", null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Description.Should().BeNull();
    }

    // ========== GetTasksAsync ==========

    [Fact]
    /// <summary>
    /// タスク一覧が正しく取得されることを検証する
    /// </summary>
    public async Task GetTasksAsync_タスク存在_タスク一覧が返される()
    {
        var userId = Guid.NewGuid();
        var tasks = new List<TaskItem>
        {
            createTaskItem(userId, "タスク1", Domain.Enums.TaskStatus.NotStarted),
            createTaskItem(userId, "タスク2", Domain.Enums.TaskStatus.InProgress)
        };
        _taskRepository.Setup(r => r.FindByUserIdAsync(userId, 1, 20, null))
            .ReturnsAsync((tasks.AsReadOnly() as IReadOnlyList<TaskItem>, 2));

        var result = await _sut.GetTasksAsync(userId, 1, 20, null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.Pagination.TotalCount.Should().Be(2);
        result.Value.Pagination.CurrentPage.Should().Be(1);
    }

    [Fact]
    /// <summary>
    /// タスク0件で空リストが返されることを検証する
    /// </summary>
    public async Task GetTasksAsync_タスク0件_空リストが返される()
    {
        var userId = Guid.NewGuid();
        _taskRepository.Setup(r => r.FindByUserIdAsync(userId, 1, 20, null))
            .ReturnsAsync((new List<TaskItem>().AsReadOnly() as IReadOnlyList<TaskItem>, 0));

        var result = await _sut.GetTasksAsync(userId, 1, 20, null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.Pagination.TotalCount.Should().Be(0);
        result.Value.Pagination.TotalPages.Should().Be(0);
    }

    [Fact]
    /// <summary>
    /// ステータスフィルタで該当タスクのみ返されることを検証する
    /// </summary>
    public async Task GetTasksAsync_ステータスフィルタ_該当タスクのみ返される()
    {
        var userId = Guid.NewGuid();
        var tasks = new List<TaskItem>
        {
            createTaskItem(userId, "進行中タスク", Domain.Enums.TaskStatus.InProgress)
        };
        _taskRepository.Setup(r => r.FindByUserIdAsync(userId, 1, 20, Domain.Enums.TaskStatus.InProgress))
            .ReturnsAsync((tasks.AsReadOnly() as IReadOnlyList<TaskItem>, 1));

        var result = await _sut.GetTasksAsync(userId, 1, 20, "IN_PROGRESS");

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items[0].Status.Should().Be("IN_PROGRESS");
    }

    [Fact]
    /// <summary>
    /// 無効なステータスでエラーが返されることを検証する
    /// </summary>
    public async Task GetTasksAsync_無効なステータス_エラーが返される()
    {
        var userId = Guid.NewGuid();

        var result = await _sut.GetTasksAsync(userId, 1, 20, "INVALID_STATUS");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TASK_INVALID_STATUS");
    }

    [Fact]
    /// <summary>
    /// ページネーションの総ページ数が正しく計算されることを検証する
    /// </summary>
    public async Task GetTasksAsync_ページネーション_総ページ数が正しい()
    {
        var userId = Guid.NewGuid();
        _taskRepository.Setup(r => r.FindByUserIdAsync(userId, 1, 10, null))
            .ReturnsAsync((new List<TaskItem>().AsReadOnly() as IReadOnlyList<TaskItem>, 25));

        var result = await _sut.GetTasksAsync(userId, 1, 10, null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Pagination.TotalPages.Should().Be(3);
        result.Value.Pagination.PerPage.Should().Be(10);
    }

    // ========== GetTaskByIdAsync ==========

    [Fact]
    /// <summary>
    /// 自分のタスクが正しく取得されることを検証する
    /// </summary>
    public async Task GetTaskByIdAsync_自分のタスク_タスクが返される()
    {
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var task = createTaskItem(userId, "テストタスク", Domain.Enums.TaskStatus.NotStarted, taskId);
        _taskRepository.Setup(r => r.FindByIdAsync(taskId)).ReturnsAsync(task);

        var result = await _sut.GetTaskByIdAsync(userId, taskId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(taskId);
        result.Value.Title.Should().Be("テストタスク");
    }

    [Fact]
    /// <summary>
    /// 他ユーザーのタスク取得が拒否されることを検証する
    /// </summary>
    public async Task GetTaskByIdAsync_他ユーザーのタスク_エラーが返される()
    {
        var ownerId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var task = createTaskItem(ownerId, "タスク", Domain.Enums.TaskStatus.NotStarted, taskId);
        _taskRepository.Setup(r => r.FindByIdAsync(taskId)).ReturnsAsync(task);

        var result = await _sut.GetTaskByIdAsync(requesterId, taskId);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TASK_NOT_FOUND");
    }

    [Fact]
    /// <summary>
    /// 存在しないタスク取得でエラーが返されることを検証する
    /// </summary>
    public async Task GetTaskByIdAsync_存在しないタスク_エラーが返される()
    {
        var taskId = Guid.NewGuid();
        _taskRepository.Setup(r => r.FindByIdAsync(taskId)).ReturnsAsync((TaskItem?)null);

        var result = await _sut.GetTaskByIdAsync(Guid.NewGuid(), taskId);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TASK_NOT_FOUND");
    }

    // ========== UpdateTaskAsync ==========

    [Fact]
    /// <summary>
    /// 有効な更新内容でタスクが更新されることを検証する
    /// </summary>
    public async Task UpdateTaskAsync_有効な更新内容_タスクが更新される()
    {
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var task = createTaskItem(userId, "旧タイトル", Domain.Enums.TaskStatus.NotStarted, taskId);
        _taskRepository.Setup(r => r.FindByIdAsync(taskId)).ReturnsAsync(task);

        var result = await _sut.UpdateTaskAsync(userId, taskId, "新タイトル", "新説明", "IN_PROGRESS");

        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("新タイトル");
        result.Value.Description.Should().Be("新説明");
        result.Value.Status.Should().Be("IN_PROGRESS");
        _taskRepository.Verify(r => r.UpdateAsync(task), Times.Once);
    }

    [Fact]
    /// <summary>
    /// 無効なステータスで更新が失敗することを検証する
    /// </summary>
    public async Task UpdateTaskAsync_無効なステータス_エラーが返される()
    {
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var task = createTaskItem(userId, "タスク", Domain.Enums.TaskStatus.NotStarted, taskId);
        _taskRepository.Setup(r => r.FindByIdAsync(taskId)).ReturnsAsync(task);

        var result = await _sut.UpdateTaskAsync(userId, taskId, "タスク", null, "INVALID");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TASK_INVALID_STATUS");
    }

    [Fact]
    /// <summary>
    /// 他ユーザーのタスク更新が拒否されることを検証する
    /// </summary>
    public async Task UpdateTaskAsync_他ユーザーのタスク_エラーが返される()
    {
        var ownerId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var task = createTaskItem(ownerId, "タスク", Domain.Enums.TaskStatus.NotStarted, taskId);
        _taskRepository.Setup(r => r.FindByIdAsync(taskId)).ReturnsAsync(task);

        var result = await _sut.UpdateTaskAsync(requesterId, taskId, "新タイトル", null, "IN_PROGRESS");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TASK_NOT_FOUND");
    }

    // ========== DeleteTaskAsync ==========

    [Fact]
    /// <summary>
    /// 自分のタスクが正しく削除されることを検証する
    /// </summary>
    public async Task DeleteTaskAsync_自分のタスク_タスクが削除される()
    {
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var task = createTaskItem(userId, "タスク", Domain.Enums.TaskStatus.NotStarted, taskId);
        _taskRepository.Setup(r => r.FindByIdAsync(taskId)).ReturnsAsync(task);

        var result = await _sut.DeleteTaskAsync(userId, taskId);

        result.IsSuccess.Should().BeTrue();
        _taskRepository.Verify(r => r.DeleteAsync(task), Times.Once);
    }

    [Fact]
    /// <summary>
    /// 他ユーザーのタスク削除が拒否されることを検証する
    /// </summary>
    public async Task DeleteTaskAsync_他ユーザーのタスク_エラーが返される()
    {
        var ownerId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var task = createTaskItem(ownerId, "タスク", Domain.Enums.TaskStatus.NotStarted, taskId);
        _taskRepository.Setup(r => r.FindByIdAsync(taskId)).ReturnsAsync(task);

        var result = await _sut.DeleteTaskAsync(requesterId, taskId);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TASK_NOT_FOUND");
        _taskRepository.Verify(r => r.DeleteAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact]
    /// <summary>
    /// 存在しないタスクの削除でエラーが返されることを検証する
    /// </summary>
    public async Task DeleteTaskAsync_存在しないタスク_エラーが返される()
    {
        var taskId = Guid.NewGuid();
        _taskRepository.Setup(r => r.FindByIdAsync(taskId)).ReturnsAsync((TaskItem?)null);

        var result = await _sut.DeleteTaskAsync(Guid.NewGuid(), taskId);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TASK_NOT_FOUND");
    }

    // ========== ステータス変換テスト ==========

    [Theory]
    [InlineData("NOT_STARTED")]
    [InlineData("IN_PROGRESS")]
    [InlineData("COMPLETED")]
    /// <summary>
    /// 全ての有効なステータス文字列でタスク更新が成功することを検証する
    /// </summary>
    public async Task UpdateTaskAsync_全有効ステータス_更新が成功する(string status)
    {
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var task = createTaskItem(userId, "タスク", Domain.Enums.TaskStatus.NotStarted, taskId);
        _taskRepository.Setup(r => r.FindByIdAsync(taskId)).ReturnsAsync(task);

        var result = await _sut.UpdateTaskAsync(userId, taskId, "タスク", null, status);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(status);
    }

    // ========== ヘルパーメソッド ==========

    /// <summary>
    /// テスト用の TaskItem を生成する
    /// </summary>
    private static TaskItem createTaskItem(Guid userId, string title, Domain.Enums.TaskStatus status, Guid? id = null)
    {
        return new TaskItem
        {
            Id = id ?? Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Description = null,
            Status = status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
