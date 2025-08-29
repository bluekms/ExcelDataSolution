namespace SchemaInfoScanner.Exceptions;

public class InvalidAttributeUsageException : Exception
{
    public InvalidAttributeUsageException(string message)
        : base(message)
    {
    }

    public InvalidAttributeUsageException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
