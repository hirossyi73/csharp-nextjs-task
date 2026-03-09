using FluentAssertions;
using TaskFlow.Infrastructure.Services;
using Xunit;

namespace TaskFlow.Tests.Unit.Infrastructure;

/// <summary>
/// PasswordHasher の単体テスト
/// </summary>
public class PasswordHasherTests
{
    private readonly PasswordHasher _sut;

    /// <summary>
    /// テストの共通セットアップ
    /// </summary>
    public PasswordHasherTests()
    {
        _sut = new PasswordHasher();
    }

    [Fact]
    /// <summary>
    /// パスワードが正しくハッシュ化されることを検証する
    /// </summary>
    public void HashPassword_有効なパスワード_ハッシュ文字列が返される()
    {
        var hash = _sut.HashPassword("Password123");

        hash.Should().NotBeNullOrEmpty();
        hash.Should().NotBe("Password123");
    }

    [Fact]
    /// <summary>
    /// 同じパスワードでも異なるハッシュが生成されることを検証する（ソルトの確認）
    /// </summary>
    public void HashPassword_同じパスワード_異なるハッシュが生成される()
    {
        var hash1 = _sut.HashPassword("Password123");
        var hash2 = _sut.HashPassword("Password123");

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    /// <summary>
    /// 正しいパスワードで検証が成功することを検証する
    /// </summary>
    public void VerifyPassword_正しいパスワード_trueが返される()
    {
        var hash = _sut.HashPassword("Password123");

        var result = _sut.VerifyPassword("Password123", hash);

        result.Should().BeTrue();
    }

    [Fact]
    /// <summary>
    /// 間違ったパスワードで検証が失敗することを検証する
    /// </summary>
    public void VerifyPassword_間違ったパスワード_falseが返される()
    {
        var hash = _sut.HashPassword("Password123");

        var result = _sut.VerifyPassword("WrongPassword", hash);

        result.Should().BeFalse();
    }
}
