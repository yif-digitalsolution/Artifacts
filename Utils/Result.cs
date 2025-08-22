using Microsoft.AspNetCore.Mvc;

namespace Utils;

public class Result
{
    public bool IsSuccess { get; }
    public string Message { get; }
    public bool IsFailure => !IsSuccess;

    protected Result(bool isSuccess, string error)
    {
        IsSuccess = isSuccess;
        Message = error;
    }

    public static Result Success(string? message = null) => new Result(true, message);

    public static Result Failure(string error) => new Result(false, error);

    public IActionResult ToActionResult()
    {
        if (IsSuccess)
            return new OkObjectResult(Message ?? "Success");

        return new BadRequestObjectResult(Message);
    }
}
public class Result<T> : Result
{
    public T Value { get; }

    private Result(bool isSuccess, T value, string error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new Result<T>(true, value, null);

    public static Result<T> Failure(string error) => new Result<T>(false, default, error);
}
