namespace REAK.API.Middleware;

public class ValidationException : Exception
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException()
        : base("One or more validation errors occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(Dictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public ValidationException(string field, string errorMessage)
        : base("One or more validation errors occurred.")
    {
        Errors = new Dictionary<string, string[]>
        {
            { field, new[] { errorMessage } }
        };
    }
}
