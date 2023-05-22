namespace CloudStorage.Api.Exceptions;

public class PdfConvertException : Exception
{
    public PdfConvertException(string? message) : base(message)
    {
    }
}