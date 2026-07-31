namespace ParsWoW.Api.Application.Common;

/// <summary>
/// Standardized response envelope used by every controller so clients
/// see a single, predictable JSON shape regardless of the underlying
/// subsystem (DBC, Armory, Auth, Shop, …).
/// </summary>
public sealed class ApiResponse<T>
{
    public required bool Success { get; init; }
    public required int StatusCode { get; init; }
    public string? Code { get; init; }
    public string? Message { get; init; }
    public T? Data { get; init; }
    public IReadOnlyList<string>? Errors { get; init; }
    public ApiResponseTrace? Trace { get; init; }

    public static ApiResponse<T> Ok(T data, int statusCode = 200, string? code = null) =>
        new() { Success = true, StatusCode = statusCode, Code = code, Data = data };

    public static ApiResponse<T> Fail(int statusCode, string code, string message,
        IReadOnlyList<string>? errors = null, ApiResponseTrace? trace = null) =>
        new() { Success = false, StatusCode = statusCode, Code = code, Message = message,
            Errors = errors, Trace = trace };
}

public sealed record ApiResponseTrace(string TraceId, string? SpanId = null);

/// <summary>
/// Lightweight result wrapper for service layer code. Controllers translate this
/// into <see cref="ApiResponse{T}"/>.
/// </summary>
public readonly struct Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Code { get; }
    public string? Error { get; }

    private Result(bool ok, T? value, string? code, string? error)
    {
        IsSuccess = ok; Value = value; Code = code; Error = error;
    }

    public static Result<T> Success(T value) => new(true, value, null, null);
    public static Result<T> Failure(string code, string error) => new(false, default, code, error);
}

public static class Result
{
    public static Result<T> Ok<T>(T value) => Result<T>.Success(value);
    public static Result<T> Fail<T>(string code, string error) => Result<T>.Failure(code, error);
}
