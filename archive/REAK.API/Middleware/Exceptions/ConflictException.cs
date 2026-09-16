namespace REAK.API.Middleware;

public class ConflictException : Exception
{
    public ConflictException(string message)
        : base(message)
    {
    }

    public ConflictException(string resourceName, object key)
        : base($"{resourceName} with id '{key}' already exists.")
    {
    }
}
