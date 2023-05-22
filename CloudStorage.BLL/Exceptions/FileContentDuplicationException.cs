namespace CloudStorage.BLL.Exceptions;

public class FileContentDuplicationException : Exception
{
    public FileContentDuplicationException(string? message) : base(message)
    {
    }
}