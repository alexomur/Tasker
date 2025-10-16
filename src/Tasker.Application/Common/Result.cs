using System.Net;
using Tasker.Application.DTOs;

namespace Tasker.Application.Common;

public class Result
{
    public bool IsSuccess { get; }
    public HttpStatusCode StatusCode { get; }
    public string? Error { get; }
    public string? ErrorCode { get; }
    public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; }

    protected Result(bool isSuccess, HttpStatusCode statusCode, string? error = null, string? errorCode = null, IReadOnlyDictionary<string, string[]>? validationErrors = null)
    {
        IsSuccess = isSuccess;
        StatusCode = statusCode;
        Error = error;
        ErrorCode = errorCode;
        ValidationErrors = validationErrors;
    }

    public static Result Ok()
    {
        return new Result(true, HttpStatusCode.OK);
    }

    public static Result<BaseResponseDto> Ok(string message)
    {
        return Ok(new BaseResponseDto()
        {
            Message = message
        });
    }

    public static Result<T> Ok<T>(T value, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return Result<T>.Ok(value, statusCode);
    }

    public static Result BadRequest(string error, HttpStatusCode statusCode = HttpStatusCode.BadRequest, string? errorCode = null)
    {
        return new Result(false, statusCode, error, errorCode);
    }
    
    public static Result<T> BadRequest<T>(string error, HttpStatusCode statusCode = HttpStatusCode.BadRequest, string? errorCode = null)
    {
        return Result<T>.Fail(error, statusCode, errorCode);
    }

    public static Result NotFound(string? message = null)
    {
        return new Result(false, HttpStatusCode.NotFound, message ?? "Not found.");
    }

    public static Result<T> NotFound<T>(string? message = null)
    {
        return Result<T>.NotFound(message);
    }

    public static Result Unauthorized(string? message = null)
    {
        return new Result(false, HttpStatusCode.Unauthorized, message ?? "Unauthorized.");
    }

    public static Result Forbidden(string? message = null)
    {
        return new Result(false, HttpStatusCode.Forbidden, message ?? "Forbidden.");
    }

    public static Result InternalError(string? message = null)
    {
        return new Result(false, HttpStatusCode.InternalServerError, message ?? "Internal server error.");
    }
}

public class Result<T> : Result
{
    public T? Value { get; }

    protected Result(bool isSuccess, HttpStatusCode statusCode, T? value = default, string? error = null, string? errorCode = null, IReadOnlyDictionary<string, string[]>? validationErrors = null)
        : base(isSuccess, statusCode, error, errorCode, validationErrors)
    {
        Value = value;
    }

    public static Result<T> Ok(T value, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new Result<T>(true, statusCode, value);
    }

    public static Result<T> Created(T value)
    {
        return new Result<T>(true, HttpStatusCode.Created, value);
    }

    public new static Result<T> Fail(string error, HttpStatusCode statusCode = HttpStatusCode.BadRequest, string? errorCode = null)
    {
        return new Result<T>(false, statusCode, default, error, errorCode);
    }

    public new static Result<T> ValidationFailed(IReadOnlyDictionary<string, string[]> errors)
    {
        return new Result<T>(false, HttpStatusCode.BadRequest, default, "Validation failed.", "validation_failed", errors);
    }

    public new static Result<T> NotFound(string? message = null)
    {
        return new Result<T>(false, HttpStatusCode.NotFound, default, message ?? "Not found.");
    }

    public new static Result<T> Unauthorized(string? message = null)
    {
        return new Result<T>(false, HttpStatusCode.Unauthorized, default, message ?? "Unauthorized.");
    }

    public new static Result<T> Forbidden(string? message = null)
    {
        return new Result<T>(false, HttpStatusCode.Forbidden, default, message ?? "Forbidden.");
    }

    public new static Result<T> InternalError(string? message = null)
    {
        return new Result<T>(false, HttpStatusCode.InternalServerError, default, message ?? "Internal server error.");
    }
}

