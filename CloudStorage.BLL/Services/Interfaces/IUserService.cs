using CloudStorage.BLL.Models;

namespace CloudStorage.BLL.Services.Interfaces;

public interface IUserService
{
    User Current { get; }
}