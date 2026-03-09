using Microsoft.Extensions.Logging;

namespace TaskFlow.Infrastructure.Data;

/// <summary>
/// データベースの初期データ投入を行うシーダー
/// </summary>
public class DatabaseSeeder
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(AppDbContext dbContext, ILogger<DatabaseSeeder> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// シードデータを投入する。べき等に動作し、既存データがある場合はスキップする
    /// </summary>
    public async Task SeedAsync()
    {
        _logger.LogInformation("データベースシーダーを実行中...");

        // シードデータが必要になったらここに追加する

        _logger.LogInformation("データベースシーダーが完了しました");
    }
}
