namespace MCP.Domain.Exceptions;

/// <summary>
///     Base class for all domain exceptions
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }

    protected DomainException(string message, Exception? innerException) : base(message, innerException)
    {
    }
}

/// <summary>
///     Generic domain exception for general domain-related errors
/// </summary>
public class GenericDomainException : DomainException
{
    public GenericDomainException(string message) : base(message)
    {
    }

    public GenericDomainException(string message, Exception? innerException) : base(message, innerException)
    {
    }
}

/// <summary>
///     Exception thrown when a domain entity is not found
/// </summary>
public class EntityNotFoundException(string entityName, object id)
    : DomainException($"Entity '{entityName}' with id '{id}' was not found.");

/// <summary>
///     Exception thrown when a domain business rule is violated
/// </summary>
public class BusinessRuleViolationException(string rule) : DomainException($"Business rule violation: {rule}");

/// <summary>
///     Exception thrown when an authentication or authorization error occurs
/// </summary>
public class AuthenticationException(string message) : DomainException($"Authentication failed: {message}");

/// <summary>
///     Exception thrown when a network or external service error occurs
/// </summary>
public class ExternalServiceException : DomainException
{
    public string ServiceName { get; }

    public ExternalServiceException(string serviceName, string message)
        : base($"External service '{serviceName}' error: {message}")
    {
        ServiceName = serviceName;
    }

    public ExternalServiceException(string serviceName, string message, Exception innerException)
        : base($"External service '{serviceName}' error: {message}", innerException)
    {
        ServiceName = serviceName;
    }
}

/// <summary>
///     Exception thrown when a configuration or setup error occurs
/// </summary>
public class ConfigurationException : DomainException
{
    public ConfigurationException(string message)
        : base($"Configuration error: {message}")
    {
    }

    public ConfigurationException(string message, Exception innerException)
        : base($"Configuration error: {message}", innerException)
    {
    }
}

/// <summary>
///     Exception thrown when a validation error occurs
/// </summary>
public class ValidationException(string propertyName, string message)
    : DomainException($"Validation failed for '{propertyName}': {message}")
{
    public string PropertyName { get; } = propertyName;
}
