using System.Net;
using System.Text.Json;

namespace TaskFlow.Api.Middleware;

/// <summary>
/// 未処理例外をキャッチし、統一的なエラーレスポンスを返すミドルウェア
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    /// <summary>
    /// ExceptionHandlingMiddleware を生成する
    /// </summary>
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// リクエストパイプラインを実行し、例外を処理する
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "予期しないエラーが発生しました: {Message}", ex.Message);
            await handleExceptionAsync(context);
        }
    }

    /// <summary>
    /// 例外を HTTP エラーレスポンスに変換する
    /// </summary>
    private static async Task handleExceptionAsync(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new
        {
            error = new
            {
                code = "INTERNAL_ERROR",
                message = "内部エラーが発生しました"
            }
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsJsonAsync(response, options);
    }
}
