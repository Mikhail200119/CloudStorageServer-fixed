namespace CloudStorage.BLL.Exceptions;

public class FileNameDuplicationException : Exception
{
    public FileNameDuplicationException(string message) : base(message)
    {
    }
}