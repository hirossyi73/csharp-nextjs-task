using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using TaskFlow.Application.Common;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Services;
using Xunit;

namespace TaskFlow.Tests.Integration.Controllers;

/// <summary>
/// AuthController の統合テスト
/// </summary>
public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly Mock<AuthServiceInterface> _mockAuthService;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>
    /// テストの共通セットアップ
    /// </summary>
    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _mockAuthService = factory.MockAuthService;
        _client = factory.CreateClient();
    }

    [Fact]
    /// <summary>
    /// 有効なメアドでユーザー仮登録が成功し 200 が返されることを検証する
    /// </summary>
    public async Task Register_有効なメール_200が返される()
    {
        _mockAuthService.Setup(s => s.RegisterAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Success());

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "test@example.com"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("確認メールを送信しました");
    }

    [Fact]
    /// <summary>
    /// メアド未入力で登録した場合に 422 が返されることを検証する
    /// </summary>
    public async Task Register_メアド未入力_422が返される()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = ""
        });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    /// <summary>
    /// 有効な認証情報でログインが成功しトークンが返されることを検証する
    /// </summary>
    public async Task Login_有効な認証情報_200とトークンが返される()
    {
        _mockAuthService.Setup(s => s.LoginAsync("test@example.com", "Password123"))
            .ReturnsAsync(Result<LoginResult>.Success(new LoginResult("access-token", "refresh-token", 900)));

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "test@example.com",
            password = "Password123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("access-token");
        // httpOnly Cookie でリフレッシュトークンが設定される
        response.Headers.TryGetValues("Set-Cookie", out var cookies);
        cookies.Should().NotBeNull();
        cookies!.Should().Contain(c => c.Contains("refreshToken"));
    }

    [Fact]
    /// <summary>
    /// 不正な認証情報でログインが失敗し 401 が返されることを検証する
    /// </summary>
    public async Task Login_不正な認証情報_401が返される()
    {
        _mockAuthService.Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Result<LoginResult>.Failure(
                new AppError("AUTH_CREDENTIALS_INVALID", "メールアドレスまたはパスワードが正しくありません")));

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "test@example.com",
            password = "wrong"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("AUTH_CREDENTIALS_INVALID");
    }

    [Fact]
    /// <summary>
    /// 認証なしで保護されたエンドポイントにアクセスすると 401 が返されることを検証する
    /// </summary>
    public async Task ProtectedEndpoint_トークンなし_401が返される()
    {
        var response = await _client.PostAsync("/api/v1/auth/logout", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
