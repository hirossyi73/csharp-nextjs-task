using FluentAssertions;
using Microsoft.Extensions.Configuration;
using TaskFlow.Infrastructure.Services;
using Xunit;

namespace TaskFlow.Tests.Unit.Infrastructure;

/// <summary>
/// JwtService の単体テスト
/// </summary>
public class JwtServiceTests
{
    private readonly IConfiguration _configuration;
    private readonly JwtService _sut;

    /// <summary>
    /// テストの共通セットアップ
    /// </summary>
    public JwtServiceTests()
    {
        var configData = new Dictionary<string, string?>
        {
            ["Jwt:SecretKey"] = "test-secret-key-at-least-32-characters-long-for-hmac256",
            ["Jwt:Issuer"] = "test-issuer",
            ["Jwt:Audience"] = "test-audience",
            ["Jwt:AccessTokenExpirationMinutes"] = "15"
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
        _sut = new JwtService(_configuration);
    }

    [Fact]
    /// <summary>
    /// アクセストークンが正常に生成されることを検証する
    /// </summary>
    public void GenerateAccessToken_有効な入力_JWT文字列が返される()
    {
        var userId = Guid.NewGuid();

        var token = _sut.GenerateAccessToken(userId, "test@example.com");

        token.Should().NotBeNullOrEmpty();
        // JWT は3つのドット区切りセグメントを持つ
        token.Split('.').Should().HaveCount(3);
    }

    [Fact]
    /// <summary>
    /// 生成したトークンからユーザー ID が正しく取得できることを検証する
    /// </summary>
    public void GetUserIdFromToken_有効なトークン_ユーザーIDが返される()
    {
        var userId = Guid.NewGuid();
        var token = _sut.GenerateAccessToken(userId, "test@example.com");

        var extractedId = _sut.GetUserIdFromToken(token);

        extractedId.Should().Be(userId);
    }

    [Fact]
    /// <summary>
    /// 無効なトークンで null が返されることを検証する
    /// </summary>
    public void GetUserIdFromToken_無効なトークン_nullが返される()
    {
        var result = _sut.GetUserIdFromToken("invalid.token.string");

        result.Should().BeNull();
    }

    [Fact]
    /// <summary>
    /// 異なる秘密鍵で署名されたトークンが拒否されることを検証する
    /// </summary>
    public void GetUserIdFromToken_異なる秘密鍵_nullが返される()
    {
        var otherConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "different-secret-key-also-at-least-32-characters-long!!",
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:Audience"] = "test-audience",
                ["Jwt:AccessTokenExpirationMinutes"] = "15"
            })
            .Build();
        var otherService = new JwtService(otherConfig);
        var token = otherService.GenerateAccessToken(Guid.NewGuid(), "test@example.com");

        var result = _sut.GetUserIdFromToken(token);

        result.Should().BeNull();
    }

    [Fact]
    /// <summary>
    /// リフレッシュトークンが毎回異なる値で生成されることを検証する
    /// </summary>
    public void GenerateRefreshToken_呼び出し毎に異なるトークンが返される()
    {
        var token1 = _sut.GenerateRefreshToken();
        var token2 = _sut.GenerateRefreshToken();

        token1.Should().NotBe(token2);
        token1.Should().NotBeNullOrEmpty();
    }

    [Fact]
    /// <summary>
    /// アクセストークンの有効期限秒数が正しく計算されることを検証する
    /// </summary>
    public void GetAccessTokenExpirationSeconds_設定値15分_900秒が返される()
    {
        var seconds = _sut.GetAccessTokenExpirationSeconds();

        seconds.Should().Be(900);
    }

    [Fact]
    /// <summary>
    /// SecretKey 未設定時に例外がスローされることを検証する
    /// </summary>
    public void GenerateAccessToken_SecretKey未設定_例外がスローされる()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();
        var service = new JwtService(config);

        var act = () => service.GenerateAccessToken(Guid.NewGuid(), "test@example.com");

        act.Should().Throw<InvalidOperationException>();
    }
}
