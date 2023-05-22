namespace CloudStorage.BLL.Exceptions;

public class UserRegisterException : Exception
{
    public UserRegisterException(string? message) : base(message)
    {
    }

    public override string Message { get; }
}