using MCP.Domain.Exceptions;

namespace MCP.Domain.Common;

public abstract record Result<T>
{
    public abstract bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public abstract T Value { get; }
    public abstract DomainException Error { get; }

    public static Result<T> Success(T value)
    {
        return new SuccessResult<T>(value);
    }

    public static Result<T> Failure(DomainException error)
    {
        return new FailureResult<T>(error);
    }


    public static Result<T> Failure(string errorMessage, Exception? error = null)
    {
        return new FailureResult<T>(new GenericDomainException(errorMessage, error));
    }

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<DomainException, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(Value) : onFailure(Error);
    }

    public async Task<TResult> MatchAsync<TResult>(Func<T, Task<TResult>> onSuccess, Func<DomainException, TResult> onFailure)
    {
        return IsSuccess ? await onSuccess(Value) : onFailure(Error);
    }
    public async Task<TResult> MatchAsync<TResult>(Func<T, Task<TResult>> onSuccess,
        Func<DomainException, Task<TResult>> onFailure)
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

internal record SuccessResult<T>(T Value) : Result<T>
{
    public override bool IsSuccess => true;
    public override T Value { get; } = Value;
    public override DomainException Error => throw new InvalidOperationException("Success result has no error");
}

internal record FailureResult<T>(DomainException Error) : Result<T>
{
    public override bool IsSuccess => false;
    public override T Value => throw new InvalidOperationException("Failure result has no value");
    public override DomainException Error { get; } = Error;
}

public static class Result
{
    public static Result<T> Success<T>(T value)
    {
        return Result<T>.Success(value);
    }

    public static Result<T> Failure<T>(DomainException error)
    {
        return Result<T>.Failure(error);
    }

    public static Result<T> Failure<T>(string errorMessage, Exception? error = null)
    {
        return Result<T>.Failure(errorMessage, error);
    }

    public static Result<Unit> Success()
    {
        return Result<Unit>.Success(Unit.Value);
    }

    public static Result<Unit> Failure(DomainException error)
    {
        return Result<Unit>.Failure(error);
    }

    public static Result<Unit> Failure(string errorMessage, Exception? error)
    {
        return Result<Unit>.Failure(errorMessage, error);
    }
}

public record Unit
{
    public static readonly Unit Value = new();
    private Unit() { }
}
