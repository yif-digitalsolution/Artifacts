namespace Utils;

public enum ResultErrorType
{
    None = 0,
    Validation = 1,
    NotFound = 2,
    Forbidden = 3,
    Error = 4
}

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public ResultErrorType ErrorType { get; }

    protected Result(bool isSuccess, string? error, ResultErrorType errorType)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorType = errorType;
    }

    public static Result Ok() => new(true, null, ResultErrorType.None);
    public static Result Fail(string error) => new(false, error, ResultErrorType.Error);
    public static Result Validation(string error) => new(false, error, ResultErrorType.Validation);
    public static Result NotFound(string error) => new(false, error, ResultErrorType.NotFound);
    public static Result Forbidden(string error) => new(false, error, ResultErrorType.Forbidden);
}

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, T? value, string? error, ResultErrorType errorType)
        : base(isSuccess, error, errorType)
    {
        Value = value;
    }

    public static Result<T> Ok(T value) => new(true, value, null, ResultErrorType.None);
    public static new Result<T> Fail(string error) => new(false, default, error, ResultErrorType.Error);
    public static new Result<T> Validation(string error) => new(false, default, error, ResultErrorType.Validation);
    public static new Result<T> NotFound(string error) => new(false, default, error, ResultErrorType.NotFound);
    public static new Result<T> Forbidden(string error) => new(false, default, error, ResultErrorType.Forbidden);
}
