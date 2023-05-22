using CloudStorage.DAL.Entities;

namespace CloudStorage.DAL.Repositories.Interfaces;

public interface IUsersRepository
{
    void Create(UserDbModel userDbModel);
    Task<UserDbModel?> GetByEmail(string email);
}