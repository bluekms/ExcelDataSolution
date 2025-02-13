namespace SchemaInfoScanner.Exceptions;

public class AttributeNotFoundException<TAttribute> : Exception
{
    public AttributeNotFoundException(string parameterName)
        : base($"The attribute '{typeof(TAttribute).Name}' was not found on {parameterName}.")
    {
    }

    public AttributeNotFoundException(string parameterName, Exception innerException)
        : base($"The attribute '{typeof(TAttribute).Name}' was not found on {parameterName}.", innerException)
    {
    }
}
