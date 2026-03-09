using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TaskFlow.Application.Interfaces;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Tests.Integration;

/// <summary>
/// 統合テスト用の WebApplicationFactory。DB をインメモリに差し替え、外部サービスをモックにする
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    /// <summary>
    /// テスト用 JWT 秘密鍵。テストコード側でトークン生成する際も同じ値を使う
    /// </summary>
    public const string TestJwtSecretKey = "test-secret-key-for-integration-tests-must-be-long-enough";

    /// <summary>
    /// モック化された AuthService
    /// </summary>
    public Mock<AuthServiceInterface> MockAuthService { get; } = new();

    /// <summary>
    /// モック化された TaskService
    /// </summary>
    public Mock<TaskServiceInterface> MockTaskService { get; } = new();

    /// <summary>
    /// テスト用のホスト構成をカスタマイズする
    /// </summary>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // JWT 設定をテスト用に上書きする（UseSetting は ConfigureAppConfiguration より先に評価される）
        builder.UseSetting("Jwt:SecretKey", TestJwtSecretKey);
        builder.UseSetting("Jwt:Issuer", "taskflow-api");
        builder.UseSetting("Jwt:Audience", "taskflow-frontend");
        builder.UseSetting("Jwt:AccessTokenExpirationMinutes", "15");
        builder.UseSetting("ConnectionStrings:DefaultConnection", "Host=localhost;Database=test");

        builder.ConfigureServices(services =>
        {
            // DbContext をインメモリ DB に差し替える
            var dbDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (dbDescriptor != null) services.Remove(dbDescriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid()));

            // サービスをモックに差し替える
            replaceService<AuthServiceInterface>(services, MockAuthService.Object);
            replaceService<TaskServiceInterface>(services, MockTaskService.Object);
        });
    }

    /// <summary>
    /// DI コンテナ内のサービス登録をモックに差し替える
    /// </summary>
    private static void replaceService<T>(IServiceCollection services, T mock) where T : class
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(T));
        if (descriptor != null) services.Remove(descriptor);
        services.AddScoped(_ => mock);
    }
}
