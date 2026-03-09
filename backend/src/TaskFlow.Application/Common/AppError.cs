namespace TaskFlow.Application.Common;

/// <summary>
/// アプリケーション層のビジネスエラー情報を表す
/// </summary>
public sealed class AppError
{
    /// <summary>
    /// エラーコード。CATEGORY_DETAIL 形式で記載する（例: AUTH_CREDENTIALS_INVALID）
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// ユーザー向けエラーメッセージ
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// バリデーションエラーの詳細。キーはフィールド名、値はエラーメッセージの配列
    /// </summary>
    public Dictionary<string, string[]>? Details { get; }

    /// <summary>
    /// AppError を生成する
    /// </summary>
    public AppError(string code, string message, Dictionary<string, string[]>? details = null)
    {
        Code = code;
        Message = message;
        Details = details;
    }

    /// <summary>
    /// バリデーションエラーを生成する
    /// </summary>
    public static AppError Validation(Dictionary<string, string[]> details)
    {
        return new AppError("VALIDATION_ERROR", "入力値が不正です", details);
    }
}
