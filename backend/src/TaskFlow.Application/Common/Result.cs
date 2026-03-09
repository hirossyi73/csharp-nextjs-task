namespace TaskFlow.Application.Common;

/// <summary>
/// 値を返さない操作の結果を表す。成功またはエラー情報のどちらかを保持する
/// </summary>
public sealed class Result
{
    private readonly AppError? _error;

    private Result(bool isSuccess, AppError? error)
    {
        IsSuccess = isSuccess;
        _error = error;
    }

    /// <summary>
    /// 成功かどうか
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// 失敗かどうか
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// エラー情報。成功時にアクセスすると例外をスローする
    /// </summary>
    public AppError Error => IsSuccess
        ? throw new InvalidOperationException("成功した Result にエラー情報は存在しない")
        : _error!;

    /// <summary>
    /// 成功 Result を生成する
    /// </summary>
    public static Result Success() => new(true, null);

    /// <summary>
    /// 失敗 Result を生成する
    /// </summary>
    public static Result Failure(AppError error) => new(false, error);
}

/// <summary>
/// 値を返す操作の結果を表す。成功値またはエラー情報のどちらかを保持する
/// </summary>
/// <typeparam name="T">成功時の値の型</typeparam>
public sealed class Result<T>
{
    private readonly T? _value;
    private readonly AppError? _error;

    private Result(bool isSuccess, T? value, AppError? error)
    {
        IsSuccess = isSuccess;
        _value = value;
        _error = error;
    }

    /// <summary>
    /// 成功かどうか
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// 失敗かどうか
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// 成功時の値。失敗時にアクセスすると例外をスローする
    /// </summary>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("失敗した Result から値は取得できない");

    /// <summary>
    /// エラー情報。成功時にアクセスすると例外をスローする
    /// </summary>
    public AppError Error => IsSuccess
        ? throw new InvalidOperationException("成功した Result にエラー情報は存在しない")
        : _error!;

    /// <summary>
    /// 成功 Result を生成する
    /// </summary>
    public static Result<T> Success(T value) => new(true, value, null);

    /// <summary>
    /// 失敗 Result を生成する
    /// </summary>
    public static Result<T> Failure(AppError error) => new(false, default, error);
}
