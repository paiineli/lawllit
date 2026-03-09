namespace Lawllit.Web;

public class Result
{
    public bool IsSuccess { get; }
    public string? ErrorKey { get; }

    protected Result(bool isSuccess, string? errorKey)
    {
        IsSuccess = isSuccess;
        ErrorKey = errorKey;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(string errorKey) => new(false, errorKey);
}

public sealed class Result<TValue> : Result
{
    public TValue? Value { get; }

    private Result(TValue? value, bool isSuccess, string? errorKey) : base(isSuccess, errorKey)
    {
        Value = value;
    }

    public static Result<TValue> Success(TValue value) => new(value, true, null);
    public new static Result<TValue> Failure(string errorKey) => new(default, false, errorKey);
}
