namespace SchemaInfoScanner.Exceptions;

public class TypeNotSupportedException : Exception
{
    public TypeNotSupportedException()
    {
    }

    public TypeNotSupportedException(string message)
        : base(message)
    {
    }

    public TypeNotSupportedException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
