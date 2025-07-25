namespace MCP.Domain.Exceptions;

/// <summary>
///     Base class for all domain exceptions
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }

    protected DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
///     Exception thrown when a domain entity is not found
/// </summary>
public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, object id)
        : base($"Entity '{entityName}' with id '{id}' was not found.")
    {
    }
}

/// <summary>
///     Exception thrown when a domain business rule is violated
/// </summary>
public class BusinessRuleViolationException : DomainException
{
    public BusinessRuleViolationException(string rule)
        : base($"Business rule violation: {rule}")
    {
    }
}
