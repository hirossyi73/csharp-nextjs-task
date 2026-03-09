using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using TaskFlow.Application.Common;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Services;
using TaskFlow.Infrastructure.Services;
using Xunit;

namespace TaskFlow.Tests.Integration.Controllers;

/// <summary>
/// TasksController の統合テスト
/// </summary>
public class TasksControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly Mock<TaskServiceInterface> _mockTaskService;
    private readonly Guid _testUserId = Guid.NewGuid();

    /// <summary>
    /// テストの共通セットアップ
    /// </summary>
    public TasksControllerTests(CustomWebApplicationFactory factory)
    {
        _mockTaskService = factory.MockTaskService;
        _client = factory.CreateClient();
    }

    [Fact]
    /// <summary>
    /// 認証済みユーザーがタスクを作成し 201 が返されることを検証する
    /// </summary>
    public async Task CreateTask_認証済み_201が返される()
    {
        var taskResult = new TaskResult(Guid.NewGuid(), "新しいタスク", "説明", "NOT_STARTED", DateTime.UtcNow, DateTime.UtcNow);
        _mockTaskService.Setup(s => s.CreateTaskAsync(It.IsAny<Guid>(), "新しいタスク", "説明"))
            .ReturnsAsync(Result<TaskResult>.Success(taskResult));

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/tasks")
        {
            Content = JsonContent.Create(new { title = "新しいタスク", description = "説明" })
        };
        addAuthHeader(request);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("新しいタスク");
    }

    [Fact]
    /// <summary>
    /// 認証済みユーザーがタスク一覧を取得し 200 が返されることを検証する
    /// </summary>
    public async Task GetTasks_認証済み_200が返される()
    {
        var tasks = new List<TaskResult>
        {
            new(Guid.NewGuid(), "タスク1", null, "NOT_STARTED", DateTime.UtcNow, DateTime.UtcNow)
        };
        var listResult = new TaskListResult(tasks, new PaginationResult(1, 20, 1, 1));
        _mockTaskService.Setup(s => s.GetTasksAsync(It.IsAny<Guid>(), 1, 20, null))
            .ReturnsAsync(Result<TaskListResult>.Success(listResult));

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/tasks");
        addAuthHeader(request);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("タスク1");
    }

    [Fact]
    /// <summary>
    /// 認証済みユーザーがタスク詳細を取得し 200 が返されることを検証する
    /// </summary>
    public async Task GetTask_認証済み_200が返される()
    {
        var taskId = Guid.NewGuid();
        var taskResult = new TaskResult(taskId, "タスク詳細", "説明文", "IN_PROGRESS", DateTime.UtcNow, DateTime.UtcNow);
        _mockTaskService.Setup(s => s.GetTaskByIdAsync(It.IsAny<Guid>(), taskId))
            .ReturnsAsync(Result<TaskResult>.Success(taskResult));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/tasks/{taskId}");
        addAuthHeader(request);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("タスク詳細");
    }

    [Fact]
    /// <summary>
    /// 認証済みユーザーがタスクを更新し 200 が返されることを検証する
    /// </summary>
    public async Task UpdateTask_認証済み_200が返される()
    {
        var taskId = Guid.NewGuid();
        var taskResult = new TaskResult(taskId, "更新後", null, "COMPLETED", DateTime.UtcNow, DateTime.UtcNow);
        _mockTaskService.Setup(s => s.UpdateTaskAsync(It.IsAny<Guid>(), taskId, "更新後", null, "COMPLETED"))
            .ReturnsAsync(Result<TaskResult>.Success(taskResult));

        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/tasks/{taskId}")
        {
            Content = JsonContent.Create(new { title = "更新後", description = (string?)null, status = "COMPLETED" })
        };
        addAuthHeader(request);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    /// <summary>
    /// 認証済みユーザーがタスクを削除し 204 が返されることを検証する
    /// </summary>
    public async Task DeleteTask_認証済み_204が返される()
    {
        var taskId = Guid.NewGuid();
        _mockTaskService.Setup(s => s.DeleteTaskAsync(It.IsAny<Guid>(), taskId))
            .ReturnsAsync(Result.Success());

        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/tasks/{taskId}");
        addAuthHeader(request);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    /// <summary>
    /// 他ユーザーのタスク取得で 404 が返されることを検証する
    /// </summary>
    public async Task GetTask_他ユーザーのタスク_404が返される()
    {
        var taskId = Guid.NewGuid();
        _mockTaskService.Setup(s => s.GetTaskByIdAsync(It.IsAny<Guid>(), taskId))
            .ReturnsAsync(Result<TaskResult>.Failure(
                new AppError("TASK_NOT_FOUND", "タスクが見つかりません")));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/tasks/{taskId}");
        addAuthHeader(request);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    /// <summary>
    /// 認証なしでタスク一覧にアクセスすると 401 が返されることを検証する
    /// </summary>
    public async Task GetTasks_認証なし_401が返される()
    {
        var response = await _client.GetAsync("/api/v1/tasks");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// テスト用の JWT アクセストークンを生成してリクエストヘッダーに設定する
    /// </summary>
    private void addAuthHeader(HttpRequestMessage request)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "test-secret-key-for-integration-tests-must-be-long-enough",
                ["Jwt:Issuer"] = "taskflow-api",
                ["Jwt:Audience"] = "taskflow-frontend",
                ["Jwt:AccessTokenExpirationMinutes"] = "15"
            })
            .Build();

        var jwtService = new JwtService(config);
        var token = jwtService.GenerateAccessToken(_testUserId, "test@example.com");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}
