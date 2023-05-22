namespace CloudStorage.BLL.Services.Interfaces;

public interface IUsersManager
{
    Task Register(string email, string password);
    Task Login(string email, string password);
}