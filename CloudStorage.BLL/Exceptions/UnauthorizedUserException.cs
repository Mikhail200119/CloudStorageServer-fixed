namespace CloudStorage.BLL.Exceptions;

public class UnauthorizedUserException : Exception
{
    public UnauthorizedUserException(string? message) : base(message)
    {
    }
}