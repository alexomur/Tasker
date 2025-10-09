namespace Tasker.Application.Common;

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }

    protected Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Ok()
    {
        return new Result(true, null);
    }

    public static Result Fail(string error)
    {
        return new Result(false, error);
    }

    public static Result<T> Ok<T>(T value)
    {
        return Result<T>.Ok(value);
    }

    public static Result<T> Fail<T>(string error)
    {
        return Result<T>.Fail(error);
    }
}

public class Result<T> : Result
{
    public T? Value { get; }

    protected Result(bool isSuccess, T? value, string? error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public static Result<T> Ok(T value)
    {
        return new Result<T>(true, value, null);
    }

    public static Result<T> Fail(string error)
    {
        return new Result<T>(false, default, error);
    }
}