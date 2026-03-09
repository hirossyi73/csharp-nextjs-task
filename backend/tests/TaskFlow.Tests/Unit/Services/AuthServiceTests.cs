using FluentAssertions;
using Moq;
using TaskFlow.Application.Common;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Services;
using TaskFlow.Domain.Entities;
using Xunit;

namespace TaskFlow.Tests.Unit.Services;

/// <summary>
/// AuthService の単体テスト
/// </summary>
public class AuthServiceTests
{
    private readonly Mock<UserRepositoryInterface> _userRepository;
    private readonly Mock<RefreshTokenRepositoryInterface> _refreshTokenRepository;
    private readonly Mock<EmailVerificationTokenRepositoryInterface> _emailVerificationTokenRepository;
    private readonly Mock<PasswordResetTokenRepositoryInterface> _passwordResetTokenRepository;
    private readonly Mock<PasswordHasherInterface> _passwordHasher;
    private readonly Mock<JwtServiceInterface> _jwtService;
    private readonly Mock<EmailServiceInterface> _emailService;
    private readonly AuthService _sut;

    /// <summary>
    /// テストの共通セットアップ
    /// </summary>
    public AuthServiceTests()
    {
        _userRepository = new Mock<UserRepositoryInterface>();
        _refreshTokenRepository = new Mock<RefreshTokenRepositoryInterface>();
        _emailVerificationTokenRepository = new Mock<EmailVerificationTokenRepositoryInterface>();
        _passwordResetTokenRepository = new Mock<PasswordResetTokenRepositoryInterface>();
        _passwordHasher = new Mock<PasswordHasherInterface>();
        _jwtService = new Mock<JwtServiceInterface>();
        _emailService = new Mock<EmailServiceInterface>();

        _sut = new AuthService(
            _userRepository.Object,
            _refreshTokenRepository.Object,
            _emailVerificationTokenRepository.Object,
            _passwordResetTokenRepository.Object,
            _passwordHasher.Object,
            _jwtService.Object,
            _emailService.Object);
    }

    // ========== RegisterAsync ==========

    [Fact]
    /// <summary>
    /// 有効なメアドで仮登録が成功し、ユーザー作成とメール送信が行われることを検証する
    /// </summary>
    public async Task RegisterAsync_有効なメール_仮登録成功しメール送信される()
    {
        _userRepository.Setup(r => r.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        _userRepository.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);
        _emailVerificationTokenRepository.Setup(r => r.CreateAsync(It.IsAny<EmailVerificationToken>()))
            .ReturnsAsync((EmailVerificationToken t) => t);

        var result = await _sut.RegisterAsync("test@example.com");

        result.IsSuccess.Should().BeTrue();
        _userRepository.Verify(r => r.CreateAsync(It.Is<User>(u =>
            u.Email == "test@example.com" && u.PasswordHash == null && !u.IsEmailVerified)), Times.Once);
        _emailVerificationTokenRepository.Verify(r => r.CreateAsync(It.IsAny<EmailVerificationToken>()), Times.Once);
        _emailService.Verify(s => s.SendEmailVerificationAsync("test@example.com", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    /// <summary>
    /// 本登録済みメールアドレスで登録した場合にエラーが返されることを検証する
    /// </summary>
    public async Task RegisterAsync_本登録済みメールアドレス_エラーが返される()
    {
        _userRepository.Setup(r => r.FindByEmailAsync("existing@example.com"))
            .ReturnsAsync(new User { Email = "existing@example.com", PasswordHash = "hash", IsEmailVerified = true });

        var result = await _sut.RegisterAsync("existing@example.com");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AUTH_EMAIL_ALREADY_EXISTS");
        _userRepository.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    // ========== VerifyEmailAsync ==========

    [Fact]
    /// <summary>
    /// 有効なトークンとパスワードで本登録が成功することを検証する
    /// </summary>
    public async Task VerifyEmailAsync_有効なトークン_本登録成功()
    {
        var user = new User { Email = "test@example.com", PasswordHash = null, IsEmailVerified = false };
        var token = createVerificationToken(user, isUsed: false, isExpired: false);

        _emailVerificationTokenRepository.Setup(r => r.FindByTokenHashAsync(It.IsAny<string>()))
            .ReturnsAsync(token);
        _passwordHasher.Setup(h => h.HashPassword("Password123")).Returns("hashed_password");

        var result = await _sut.VerifyEmailAsync("valid-token", "Password123");

        result.IsSuccess.Should().BeTrue();
        token.IsUsed.Should().BeTrue();
        user.IsEmailVerified.Should().BeTrue();
        user.PasswordHash.Should().Be("hashed_password");
        _emailVerificationTokenRepository.Verify(r => r.UpdateAsync(token), Times.Once);
        _userRepository.Verify(r => r.UpdateAsync(user), Times.Once);
    }

    [Fact]
    /// <summary>
    /// 無効なトークンでメール確認が失敗することを検証する
    /// </summary>
    public async Task VerifyEmailAsync_無効なトークン_エラーが返される()
    {
        _emailVerificationTokenRepository.Setup(r => r.FindByTokenHashAsync(It.IsAny<string>()))
            .ReturnsAsync((EmailVerificationToken?)null);

        var result = await _sut.VerifyEmailAsync("invalid-token", "Password123");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AUTH_TOKEN_INVALID");
    }

    [Fact]
    /// <summary>
    /// 使用済みトークンでメール確認が失敗することを検証する
    /// </summary>
    public async Task VerifyEmailAsync_使用済みトークン_エラーが返される()
    {
        var user = new User { Email = "test@example.com", PasswordHash = "hash" };
        var token = createVerificationToken(user, isUsed: true, isExpired: false);

        _emailVerificationTokenRepository.Setup(r => r.FindByTokenHashAsync(It.IsAny<string>()))
            .ReturnsAsync(token);

        var result = await _sut.VerifyEmailAsync("used-token", "Password123");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AUTH_TOKEN_ALREADY_USED");
    }

    [Fact]
    /// <summary>
    /// 期限切れトークンでメール確認が失敗することを検証する
    /// </summary>
    public async Task VerifyEmailAsync_期限切れトークン_エラーが返される()
    {
        var user = new User { Email = "test@example.com", PasswordHash = "hash" };
        var token = createVerificationToken(user, isUsed: false, isExpired: true);

        _emailVerificationTokenRepository.Setup(r => r.FindByTokenHashAsync(It.IsAny<string>()))
            .ReturnsAsync(token);

        var result = await _sut.VerifyEmailAsync("expired-token", "Password123");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AUTH_TOKEN_EXPIRED");
    }


    [Fact]
    /// <summary>
    /// 仮登録済みユーザーが再登録すると確認メールが再送信されることを検証する
    /// </summary>
    public async Task RegisterAsync_仮登録済みメールアドレス_確認メールが再送信される()
    {
        var existingUser = new User { Id = Guid.NewGuid(), Email = "test@example.com", PasswordHash = null, IsEmailVerified = false };
        _userRepository.Setup(r => r.FindByEmailAsync("test@example.com"))
            .ReturnsAsync(existingUser);
        _emailVerificationTokenRepository.Setup(r => r.CreateAsync(It.IsAny<EmailVerificationToken>()))
            .ReturnsAsync((EmailVerificationToken t) => t);

        var result = await _sut.RegisterAsync("test@example.com");

        result.IsSuccess.Should().BeTrue();
        _emailVerificationTokenRepository.Verify(r => r.InvalidateByUserIdAsync(existingUser.Id), Times.Once);
        _emailService.Verify(s => s.SendEmailVerificationAsync("test@example.com", It.IsAny<string>()), Times.Once);
        _userRepository.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    /// <summary>
    /// 既存パスワードありでパスワード未提供の場合、メール確認のみ成功することを検証する（後方互換性）
    /// </summary>
    public async Task VerifyEmailAsync_既存パスワードありでパスワード未提供_メール確認のみ成功()
    {
        var user = new User { Email = "test@example.com", PasswordHash = "existing_hash", IsEmailVerified = false };
        var token = createVerificationToken(user, isUsed: false, isExpired: false);

        _emailVerificationTokenRepository.Setup(r => r.FindByTokenHashAsync(It.IsAny<string>()))
            .ReturnsAsync(token);

        var result = await _sut.VerifyEmailAsync("valid-token", null);

        result.IsSuccess.Should().BeTrue();
        user.IsEmailVerified.Should().BeTrue();
        user.PasswordHash.Should().Be("existing_hash");
    }

    [Fact]
    /// <summary>
    /// パスワード未提供かつ PasswordHash 未設定の場合、エラーが返されることを検証する
    /// </summary>
    public async Task VerifyEmailAsync_パスワード未提供かつPasswordHash未設定_エラーが返される()
    {
        var user = new User { Email = "test@example.com", PasswordHash = null, IsEmailVerified = false };
        var token = createVerificationToken(user, isUsed: false, isExpired: false);

        _emailVerificationTokenRepository.Setup(r => r.FindByTokenHashAsync(It.IsAny<string>()))
            .ReturnsAsync(token);

        var result = await _sut.VerifyEmailAsync("valid-token", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AUTH_PASSWORD_REQUIRED");
    }

    [Fact]
    /// <summary>
    /// パスワード未設定ユーザーがログインを試みるとエラーが返されることを検証する
    /// </summary>
    public async Task LoginAsync_パスワード未設定ユーザー_エラーが返される()
    {
        var user = new User { Email = "test@example.com", PasswordHash = null, IsEmailVerified = false };
        _userRepository.Setup(r => r.FindByEmailAsync("test@example.com")).ReturnsAsync(user);

        var result = await _sut.LoginAsync("test@example.com", "Password123");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AUTH_REGISTRATION_INCOMPLETE");
        _passwordHasher.Verify(h => h.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    // ========== LoginAsync ==========

    [Fact]
    /// <summary>
    /// 有効な認証情報でログインが成功しトークンが発行されることを検証する
    /// </summary>
    public async Task LoginAsync_有効な認証情報_トークンが発行される()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hashed",
            IsEmailVerified = true
        };
        _userRepository.Setup(r => r.FindByEmailAsync("test@example.com")).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.VerifyPassword("Password123", "hashed")).Returns(true);
        _jwtService.Setup(j => j.GenerateAccessToken(user.Id, user.Email)).Returns("access-token");
        _jwtService.Setup(j => j.GenerateRefreshToken()).Returns("refresh-token");
        _jwtService.Setup(j => j.GetAccessTokenExpirationSeconds()).Returns(900);
        _refreshTokenRepository.Setup(r => r.CreateAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync((RefreshToken t) => t);

        var result = await _sut.LoginAsync("test@example.com", "Password123");

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access-token");
        result.Value.RefreshToken.Should().Be("refresh-token");
        result.Value.ExpiresIn.Should().Be(900);
        _refreshTokenRepository.Verify(r => r.CreateAsync(It.IsAny<RefreshToken>()), Times.Once);
    }

    [Fact]
    /// <summary>
    /// パスワード不一致でログインが失敗することを検証する
    /// </summary>
    public async Task LoginAsync_パスワード不一致_エラーが返される()
    {
        var user = new User { Email = "test@example.com", PasswordHash = "hashed", IsEmailVerified = true };
        _userRepository.Setup(r => r.FindByEmailAsync("test@example.com")).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.VerifyPassword("wrong", "hashed")).Returns(false);

        var result = await _sut.LoginAsync("test@example.com", "wrong");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AUTH_CREDENTIALS_INVALID");
    }

    [Fact]
    /// <summary>
    /// メール未確認ユーザーがログイン拒否されることを検証する
    /// </summary>
    public async Task LoginAsync_メール未確認_エラーが返される()
    {
        var user = new User { Email = "test@example.com", PasswordHash = "hashed", IsEmailVerified = false };
        _userRepository.Setup(r => r.FindByEmailAsync("test@example.com")).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.VerifyPassword("Password123", "hashed")).Returns(true);

        var result = await _sut.LoginAsync("test@example.com", "Password123");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AUTH_EMAIL_NOT_VERIFIED");
    }

    [Fact]
    /// <summary>
    /// 存在しないメールアドレスでログインが失敗することを検証する
    /// </summary>
    public async Task LoginAsync_存在しないメール_エラーが返される()
    {
        _userRepository.Setup(r => r.FindByEmailAsync("none@example.com"))
            .ReturnsAsync((User?)null);

        var result = await _sut.LoginAsync("none@example.com", "Password123");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AUTH_CREDENTIALS_INVALID");
    }

    // ========== LogoutAsync ==========

    [Fact]
    /// <summary>
    /// ログアウト時にリフレッシュトークンが全て無効化されることを検証する
    /// </summary>
    public async Task LogoutAsync_正常_リフレッシュトークンが無効化される()
    {
        var userId = Guid.NewGuid();

        var result = await _sut.LogoutAsync(userId);

        result.IsSuccess.Should().BeTrue();
        _refreshTokenRepository.Verify(r => r.RevokeAllByUserIdAsync(userId), Times.Once);
    }

    // ========== RefreshTokenAsync ==========

    [Fact]
    /// <summary>
    /// 有効なリフレッシュトークンで新しいトークンが発行されることを検証する
    /// </summary>
    public async Task RefreshTokenAsync_有効なトークン_新しいトークンが発行される()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", PasswordHash = "hash" };
        var storedToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = "hash",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            User = user
        };
        _refreshTokenRepository.Setup(r => r.FindByTokenHashAsync(It.IsAny<string>()))
            .ReturnsAsync(storedToken);
        _jwtService.Setup(j => j.GenerateAccessToken(user.Id, user.Email)).Returns("new-access");
        _jwtService.Setup(j => j.GenerateRefreshToken()).Returns("new-refresh");
        _jwtService.Setup(j => j.GetAccessTokenExpirationSeconds()).Returns(900);
        _refreshTokenRepository.Setup(r => r.CreateAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync((RefreshToken t) => t);

        var result = await _sut.RefreshTokenAsync("old-refresh-token");

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("new-access");
        result.Value.RefreshToken.Should().Be("new-refresh");
        storedToken.IsRevoked.Should().BeTrue();
        _refreshTokenRepository.Verify(r => r.UpdateAsync(storedToken), Times.Once);
        _refreshTokenRepository.Verify(r => r.CreateAsync(It.IsAny<RefreshToken>()), Times.Once);
    }

    [Fact]
    /// <summary>
    /// 期限切れリフレッシュトークンで更新が失敗することを検証する
    /// </summary>
    public async Task RefreshTokenAsync_期限切れトークン_エラーが返される()
    {
        var storedToken = new RefreshToken
        {
            TokenHash = "hash",
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            IsRevoked = false,
            User = new User { Email = "test@example.com", PasswordHash = "hash" }
        };
        _refreshTokenRepository.Setup(r => r.FindByTokenHashAsync(It.IsAny<string>()))
            .ReturnsAsync(storedToken);

        var result = await _sut.RefreshTokenAsync("expired-token");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AUTH_TOKEN_INVALID");
    }

    [Fact]
    /// <summary>
    /// 無効化済みリフレッシュトークンで更新が失敗することを検証する
    /// </summary>
    public async Task RefreshTokenAsync_無効化済みトークン_エラーが返される()
    {
        var storedToken = new RefreshToken
        {
            TokenHash = "hash",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = true,
            User = new User { Email = "test@example.com", PasswordHash = "hash" }
        };
        _refreshTokenRepository.Setup(r => r.FindByTokenHashAsync(It.IsAny<string>()))
            .ReturnsAsync(storedToken);

        var result = await _sut.RefreshTokenAsync("revoked-token");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AUTH_TOKEN_INVALID");
    }

    // ========== ForgotPasswordAsync ==========

    [Fact]
    /// <summary>
    /// 存在するメールアドレスでリセットメールが送信されることを検証する
    /// </summary>
    public async Task ForgotPasswordAsync_存在するメール_リセットメールが送信される()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", PasswordHash = "hash" };
        _userRepository.Setup(r => r.FindByEmailAsync("test@example.com")).ReturnsAsync(user);
        _passwordResetTokenRepository.Setup(r => r.CreateAsync(It.IsAny<PasswordResetToken>()))
            .ReturnsAsync((PasswordResetToken t) => t);

        var result = await _sut.ForgotPasswordAsync("test@example.com");

        result.IsSuccess.Should().BeTrue();
        _passwordResetTokenRepository.Verify(r => r.CreateAsync(It.IsAny<PasswordResetToken>()), Times.Once);
        _emailService.Verify(s => s.SendPasswordResetAsync("test@example.com", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    /// <summary>
    /// 存在しないメールアドレスでも成功レスポンスが返される（セキュリティ対策）ことを検証する
    /// </summary>
    public async Task ForgotPasswordAsync_存在しないメール_同じ成功レスポンスが返される()
    {
        _userRepository.Setup(r => r.FindByEmailAsync("none@example.com"))
            .ReturnsAsync((User?)null);

        var result = await _sut.ForgotPasswordAsync("none@example.com");

        result.IsSuccess.Should().BeTrue();
        _passwordResetTokenRepository.Verify(r => r.CreateAsync(It.IsAny<PasswordResetToken>()), Times.Never);
        _emailService.Verify(s => s.SendPasswordResetAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    // ========== ResetPasswordAsync ==========

    [Fact]
    /// <summary>
    /// 有効なトークンと新パスワードでパスワードが更新されることを検証する
    /// </summary>
    public async Task ResetPasswordAsync_有効なトークン_パスワードが更新される()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", PasswordHash = "old_hash" };
        var resetToken = createResetToken(user, isUsed: false, isExpired: false);

        _passwordResetTokenRepository.Setup(r => r.FindByTokenHashAsync(It.IsAny<string>()))
            .ReturnsAsync(resetToken);
        _passwordHasher.Setup(h => h.HashPassword("NewPassword123")).Returns("new_hash");

        var result = await _sut.ResetPasswordAsync("valid-token", "NewPassword123");

        result.IsSuccess.Should().BeTrue();
        resetToken.IsUsed.Should().BeTrue();
        user.PasswordHash.Should().Be("new_hash");
        _refreshTokenRepository.Verify(r => r.RevokeAllByUserIdAsync(user.Id), Times.Once);
    }

    [Fact]
    /// <summary>
    /// 期限切れトークンでパスワードリセットが失敗することを検証する
    /// </summary>
    public async Task ResetPasswordAsync_期限切れトークン_エラーが返される()
    {
        var user = new User { Email = "test@example.com", PasswordHash = "hash" };
        var resetToken = createResetToken(user, isUsed: false, isExpired: true);

        _passwordResetTokenRepository.Setup(r => r.FindByTokenHashAsync(It.IsAny<string>()))
            .ReturnsAsync(resetToken);

        var result = await _sut.ResetPasswordAsync("expired-token", "NewPassword123");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AUTH_TOKEN_EXPIRED");
    }

    [Fact]
    /// <summary>
    /// 無効なトークンでパスワードリセットが失敗することを検証する
    /// </summary>
    public async Task ResetPasswordAsync_無効なトークン_エラーが返される()
    {
        _passwordResetTokenRepository.Setup(r => r.FindByTokenHashAsync(It.IsAny<string>()))
            .ReturnsAsync((PasswordResetToken?)null);

        var result = await _sut.ResetPasswordAsync("invalid-token", "NewPassword123");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AUTH_TOKEN_INVALID");
    }

    // ========== ヘルパーメソッド ==========

    /// <summary>
    /// テスト用のメール確認トークンを生成する
    /// </summary>
    private static EmailVerificationToken createVerificationToken(User user, bool isUsed, bool isExpired)
    {
        return new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = "test-hash",
            IsUsed = isUsed,
            ExpiresAt = isExpired ? DateTime.UtcNow.AddHours(-1) : DateTime.UtcNow.AddHours(24),
            CreatedAt = DateTime.UtcNow,
            User = user
        };
    }

    /// <summary>
    /// テスト用のパスワードリセットトークンを生成する
    /// </summary>
    private static PasswordResetToken createResetToken(User user, bool isUsed, bool isExpired)
    {
        return new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = "test-hash",
            IsUsed = isUsed,
            ExpiresAt = isExpired ? DateTime.UtcNow.AddHours(-1) : DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            User = user
        };
    }
}
