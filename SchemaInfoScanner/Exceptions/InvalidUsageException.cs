namespace SchemaInfoScanner.Exceptions;

public class InvalidUsageException : Exception
{
    public InvalidUsageException()
    {
    }

    public InvalidUsageException(string message)
        : base(message)
    {
    }

    public InvalidUsageException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
