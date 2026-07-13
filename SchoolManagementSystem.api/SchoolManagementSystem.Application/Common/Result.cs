namespace SchoolManagementSystem.Application.Common;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public ResultErrorType? ErrorType { get; }

    private Result(bool isSuccess, T? value, string? error, ResultErrorType? errorType)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        ErrorType = errorType;
    }

    public static Result<T> Success(T value) => new(true, value, null, null);
    public static Result<T> Failure(string error, ResultErrorType type) => new(false, default, error, type);
}

public enum ResultErrorType
{
    NotFound,
    Validation
}