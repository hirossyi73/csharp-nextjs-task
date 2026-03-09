using System.Security.Cryptography;
using System.Text;
using TaskFlow.Application.Common;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Services;

/// <summary>
/// 認証に関するビジネスロジックを提供するサービス
/// </summary>
public class AuthService : AuthServiceInterface
{
    private readonly UserRepositoryInterface _userRepository;
    private readonly RefreshTokenRepositoryInterface _refreshTokenRepository;
    private readonly EmailVerificationTokenRepositoryInterface _emailVerificationTokenRepository;
    private readonly PasswordResetTokenRepositoryInterface _passwordResetTokenRepository;
    private readonly PasswordHasherInterface _passwordHasher;
    private readonly JwtServiceInterface _jwtService;
    private readonly EmailServiceInterface _emailService;

    /// <summary>
    /// AuthService を生成する
    /// </summary>
    public AuthService(
        UserRepositoryInterface userRepository,
        RefreshTokenRepositoryInterface refreshTokenRepository,
        EmailVerificationTokenRepositoryInterface emailVerificationTokenRepository,
        PasswordResetTokenRepositoryInterface passwordResetTokenRepository,
        PasswordHasherInterface passwordHasher,
        JwtServiceInterface jwtService,
        EmailServiceInterface emailService)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _emailVerificationTokenRepository = emailVerificationTokenRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _emailService = emailService;
    }

    /// <summary>
    /// ユーザーを仮登録し、確認メールを送信する
    /// </summary>
    public async Task<Result> RegisterAsync(string email, string password)
    {
        var existingUser = await _userRepository.FindByEmailAsync(email);
        if (existingUser != null)
        {
            return Result.Failure(new AppError("AUTH_EMAIL_ALREADY_EXISTS", "このメールアドレスは既に登録されています"));
        }

        var user = new User
        {
            Email = email,
            PasswordHash = _passwordHasher.HashPassword(password),
            IsEmailVerified = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user);

        var token = generateSecureToken();
        var tokenHash = hashToken(token);

        var verificationToken = new EmailVerificationToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        await _emailVerificationTokenRepository.CreateAsync(verificationToken);
        await _emailService.SendEmailVerificationAsync(email, token);

        return Result.Success();
    }

    /// <summary>
    /// メール確認トークンを検証し、ユーザーを本登録する
    /// </summary>
    public async Task<Result> VerifyEmailAsync(string token)
    {
        var tokenHash = hashToken(token);
        var verificationToken = await _emailVerificationTokenRepository.FindByTokenHashAsync(tokenHash);

        if (verificationToken == null)
        {
            return Result.Failure(new AppError("AUTH_TOKEN_INVALID", "無効なトークンです"));
        }

        if (verificationToken.IsUsed)
        {
            return Result.Failure(new AppError("AUTH_TOKEN_ALREADY_USED", "このトークンは既に使用されています"));
        }

        if (verificationToken.ExpiresAt < DateTime.UtcNow)
        {
            return Result.Failure(new AppError("AUTH_TOKEN_EXPIRED", "トークンの有効期限が切れています"));
        }

        verificationToken.IsUsed = true;
        await _emailVerificationTokenRepository.UpdateAsync(verificationToken);

        var user = verificationToken.User;
        user.IsEmailVerified = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        return Result.Success();
    }

    /// <summary>
    /// ログイン認証を行い、アクセストークンとリフレッシュトークンを返す
    /// </summary>
    public async Task<Result<LoginResult>> LoginAsync(string email, string password)
    {
        var user = await _userRepository.FindByEmailAsync(email);

        if (user == null || !_passwordHasher.VerifyPassword(password, user.PasswordHash))
        {
            return Result<LoginResult>.Failure(
                new AppError("AUTH_CREDENTIALS_INVALID", "メールアドレスまたはパスワードが正しくありません"));
        }

        if (!user.IsEmailVerified)
        {
            return Result<LoginResult>.Failure(
                new AppError("AUTH_EMAIL_NOT_VERIFIED", "メールアドレスが確認されていません"));
        }

        var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Email);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenHash = hashToken(refreshToken);

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        await _refreshTokenRepository.CreateAsync(refreshTokenEntity);

        return Result<LoginResult>.Success(new LoginResult(
            accessToken,
            refreshToken,
            _jwtService.GetAccessTokenExpirationSeconds()));
    }

    /// <summary>
    /// ログアウト処理を行い、ユーザーの全リフレッシュトークンを無効化する
    /// </summary>
    public async Task<Result> LogoutAsync(Guid userId)
    {
        await _refreshTokenRepository.RevokeAllByUserIdAsync(userId);
        return Result.Success();
    }

    /// <summary>
    /// リフレッシュトークンを検証し、新しいアクセストークンとリフレッシュトークンを発行する
    /// </summary>
    public async Task<Result<LoginResult>> RefreshTokenAsync(string refreshToken)
    {
        var tokenHash = hashToken(refreshToken);
        var storedToken = await _refreshTokenRepository.FindByTokenHashAsync(tokenHash);

        if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
        {
            return Result<LoginResult>.Failure(
                new AppError("AUTH_TOKEN_INVALID", "無効なリフレッシュトークンです"));
        }

        // トークンローテーション: 使用済みトークンを無効化し新しいトークンを発行する
        storedToken.IsRevoked = true;
        await _refreshTokenRepository.UpdateAsync(storedToken);

        var user = storedToken.User;
        var newAccessToken = _jwtService.GenerateAccessToken(user.Id, user.Email);
        var newRefreshToken = _jwtService.GenerateRefreshToken();
        var newRefreshTokenHash = hashToken(newRefreshToken);

        var newRefreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = newRefreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        await _refreshTokenRepository.CreateAsync(newRefreshTokenEntity);

        return Result<LoginResult>.Success(new LoginResult(
            newAccessToken,
            newRefreshToken,
            _jwtService.GetAccessTokenExpirationSeconds()));
    }

    /// <summary>
    /// パスワードリセット要求を処理し、リセットメールを送信する
    /// </summary>
    public async Task<Result> ForgotPasswordAsync(string email)
    {
        var user = await _userRepository.FindByEmailAsync(email);

        // ユーザーが存在しない場合もセキュリティ上、同じレスポンスを返す
        if (user == null)
        {
            return Result.Success();
        }

        var token = generateSecureToken();
        var tokenHash = hashToken(token);

        var resetToken = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        await _passwordResetTokenRepository.CreateAsync(resetToken);
        await _emailService.SendPasswordResetAsync(email, token);

        return Result.Success();
    }

    /// <summary>
    /// パスワードリセットトークンを検証し、パスワードを再設定する
    /// </summary>
    public async Task<Result> ResetPasswordAsync(string token, string newPassword)
    {
        var tokenHash = hashToken(token);
        var resetToken = await _passwordResetTokenRepository.FindByTokenHashAsync(tokenHash);

        if (resetToken == null)
        {
            return Result.Failure(new AppError("AUTH_TOKEN_INVALID", "無効なトークンです"));
        }

        if (resetToken.IsUsed)
        {
            return Result.Failure(new AppError("AUTH_TOKEN_ALREADY_USED", "このトークンは既に使用されています"));
        }

        if (resetToken.ExpiresAt < DateTime.UtcNow)
        {
            return Result.Failure(new AppError("AUTH_TOKEN_EXPIRED", "トークンの有効期限が切れています"));
        }

        resetToken.IsUsed = true;
        await _passwordResetTokenRepository.UpdateAsync(resetToken);

        var user = resetToken.User;
        user.PasswordHash = _passwordHasher.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        // パスワード変更後、既存のリフレッシュトークンを全て無効化する
        await _refreshTokenRepository.RevokeAllByUserIdAsync(user.Id);

        return Result.Success();
    }

    /// <summary>
    /// URL セーフなセキュアランダムトークンを生成する
    /// </summary>
    private static string generateSecureToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        // URL クエリパラメータで安全に使えるよう、Base64URL 形式に変換する
        return Convert.ToBase64String(randomBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    /// <summary>
    /// トークンを SHA256 でハッシュ化する
    /// </summary>
    private static string hashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}

/// <summary>
/// ログイン成功時の結果を表す
/// </summary>
public record LoginResult(string AccessToken, string RefreshToken, int ExpiresIn);
