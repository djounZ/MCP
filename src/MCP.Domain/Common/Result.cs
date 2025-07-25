namespace MCP.Domain.Common;

public abstract record Result<T>
{
    public abstract bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public abstract T Value { get; }
    public abstract string Error { get; }

    public static Result<T> Success(T value)
    {
        return new SuccessResult<T>(value);
    }

    public static Result<T> Failure(string error)
    {
        return new FailureResult<T>(error);
    }

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(Value) : onFailure(Error);
    }

    public async Task<TResult> MatchAsync<TResult>(Func<T, Task<TResult>> onSuccess,
        Func<string, Task<TResult>> onFailure)
    {
        return IsSuccess ? await onSuccess(Value) : await onFailure(Error);
    }

    public Result<TResult> Map<TResult>(Func<T, TResult> mapper)
    {
        return IsSuccess ? Result<TResult>.Success(mapper(Value)) : Result<TResult>.Failure(Error);
    }

    public async Task<Result<TResult>> MapAsync<TResult>(Func<T, Task<TResult>> mapper)
    {
        return IsSuccess ? Result<TResult>.Success(await mapper(Value)) : Result<TResult>.Failure(Error);
    }

    public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> binder)
    {
        return IsSuccess ? binder(Value) : Result<TResult>.Failure(Error);
    }

    public async Task<Result<TResult>> BindAsync<TResult>(Func<T, Task<Result<TResult>>> binder)
    {
        return IsSuccess ? await binder(Value) : Result<TResult>.Failure(Error);
    }
}

internal record SuccessResult<T> : Result<T>
{
    public SuccessResult(T value)
    {
        Value = value;
    }

    public override bool IsSuccess => true;
    public override T Value { get; }
    public override string Error => throw new InvalidOperationException("Success result has no error");
}

internal record FailureResult<T> : Result<T>
{
    public FailureResult(string error)
    {
        Error = error;
    }

    public override bool IsSuccess => false;
    public override T Value => throw new InvalidOperationException("Failure result has no value");
    public override string Error { get; }
}

public static class Result
{
    public static Result<T> Success<T>(T value)
    {
        return Result<T>.Success(value);
    }

    public static Result<T> Failure<T>(string error)
    {
        return Result<T>.Failure(error);
    }

    public static Result<Unit> Success()
    {
        return Result<Unit>.Success(Unit.Value);
    }

    public static Result<Unit> Failure(string error)
    {
        return Result<Unit>.Failure(error);
    }
}

public record Unit
{
    public static readonly Unit Value = new();
    private Unit() { }
}
