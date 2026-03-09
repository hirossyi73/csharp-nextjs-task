using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Dtos.Auth;
using TaskFlow.Application.Common;
using TaskFlow.Application.Services;

namespace TaskFlow.Api.Controllers;

/// <summary>
/// 認証関連のエンドポイントを提供するコントローラー
/// </summary>
[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    /// <summary>
    /// AuthController を生成する
    /// </summary>
    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// ユーザー仮登録
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request.Email, request.Password);

        if (result.IsFailure)
        {
            return toErrorResponse(result.Error);
        }

        return Ok(new MessageResponse
        {
            Message = "確認メールを送信しました。メール内のリンクをクリックして登録を完了してください。"
        });
    }

    /// <summary>
    /// メール確認（本登録）
    /// </summary>
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        var result = await _authService.VerifyEmailAsync(request.Token);

        if (result.IsFailure)
        {
            return toErrorResponse(result.Error);
        }

        return Ok(new MessageResponse
        {
            Message = "メール確認が完了しました。ログインしてください。"
        });
    }

    /// <summary>
    /// ログイン
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request.Email, request.Password);

        if (result.IsFailure)
        {
            return toErrorResponse(result.Error);
        }

        var loginResult = result.Value;
        setRefreshTokenCookie(loginResult.RefreshToken);

        return Ok(new AuthResponse
        {
            AccessToken = loginResult.AccessToken,
            ExpiresIn = loginResult.ExpiresIn
        });
    }

    /// <summary>
    /// ログアウト
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = getUserIdFromClaims();
        if (userId == null)
        {
            return Unauthorized();
        }

        await _authService.LogoutAsync(userId.Value);
        deleteRefreshTokenCookie();

        return NoContent();
    }

    /// <summary>
    /// トークンリフレッシュ
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(new { error = new { code = "AUTH_TOKEN_MISSING", message = "リフレッシュトークンがありません" } });
        }

        var result = await _authService.RefreshTokenAsync(refreshToken);

        if (result.IsFailure)
        {
            deleteRefreshTokenCookie();
            return toErrorResponse(result.Error);
        }

        var loginResult = result.Value;
        setRefreshTokenCookie(loginResult.RefreshToken);

        return Ok(new AuthResponse
        {
            AccessToken = loginResult.AccessToken,
            ExpiresIn = loginResult.ExpiresIn
        });
    }

    /// <summary>
    /// パスワードリセット要求
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await _authService.ForgotPasswordAsync(request.Email);

        return Ok(new MessageResponse
        {
            Message = "パスワードリセット用のメールを送信しました。"
        });
    }

    /// <summary>
    /// パスワード再設定
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _authService.ResetPasswordAsync(request.Token, request.NewPassword);

        if (result.IsFailure)
        {
            return toErrorResponse(result.Error);
        }

        return Ok(new MessageResponse
        {
            Message = "パスワードが再設定されました。新しいパスワードでログインしてください。"
        });
    }

    /// <summary>
    /// AppError を HTTP エラーレスポンスに変換する
    /// </summary>
    private IActionResult toErrorResponse(AppError error)
    {
        var statusCode = error.Code switch
        {
            "AUTH_CREDENTIALS_INVALID" => StatusCodes.Status401Unauthorized,
            "AUTH_EMAIL_NOT_VERIFIED" => StatusCodes.Status403Forbidden,
            "AUTH_TOKEN_INVALID" => StatusCodes.Status401Unauthorized,
            "AUTH_TOKEN_EXPIRED" => StatusCodes.Status401Unauthorized,
            "AUTH_TOKEN_ALREADY_USED" => StatusCodes.Status401Unauthorized,
            "VALIDATION_ERROR" => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status409Conflict
        };

        var response = new { error = new { code = error.Code, message = error.Message, details = error.Details } };
        return StatusCode(statusCode, response);
    }

    /// <summary>
    /// リフレッシュトークンを httpOnly Cookie に設定する
    /// </summary>
    private void setRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // 開発環境では false（本番では true にする）
            SameSite = SameSiteMode.Lax,
            Path = "/api/v1/auth",
            MaxAge = TimeSpan.FromDays(7)
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }

    /// <summary>
    /// リフレッシュトークン Cookie を削除する
    /// </summary>
    private void deleteRefreshTokenCookie()
    {
        Response.Cookies.Delete("refreshToken", new CookieOptions
        {
            Path = "/api/v1/auth"
        });
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
