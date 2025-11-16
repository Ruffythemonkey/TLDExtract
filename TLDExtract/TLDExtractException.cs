namespace TLDExtract;

public class TLDExtractException : Exception
{
    public TLDExtractException() { }

    public TLDExtractException(string message) : base(message) { }

    public TLDExtractException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
