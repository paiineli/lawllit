namespace Lawllit.Model.Common;

// Carrega sucesso ou a chave da mensagem de erro, nunca o texto pronto.
// Quem traduz é a camada de apresentação, com a cultura do usuário da requisição.
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
