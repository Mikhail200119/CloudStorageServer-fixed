using System.Security.Cryptography;
using System.Text;
using CloudStorage.BLL.Exceptions;
using CloudStorage.BLL.Services.Interfaces;
using CloudStorage.DAL;
using CloudStorage.DAL.Entities;

namespace CloudStorage.BLL.Services;

public class UsersManager : IUsersManager
{
    private readonly ICloudStorageUnitOfWork _cloudStorageUnitOfWork;

    public UsersManager(ICloudStorageUnitOfWork cloudStorageUnitOfWork)
    {
        _cloudStorageUnitOfWork = cloudStorageUnitOfWork;
    }

    public async Task Register(string email, string password)
    {
        var user = await _cloudStorageUnitOfWork.Users.GetByEmail(email);

        if (user is not null)
        {
            throw new UserRegisterException($"User with email '{email}' is already registered.");
        }

        var passwordHash = string.Concat(SHA1.HashData(Encoding.ASCII.GetBytes(password))
            .Select(b => b.ToString("X2")));

        _cloudStorageUnitOfWork.Users.Create(new UserDbModel
        {
            Email = email,
            PasswordHash = passwordHash
        });

        await _cloudStorageUnitOfWork.SaveChangesAsync();
    }

    public async Task Login(string email, string password)
    {
        var user = await _cloudStorageUnitOfWork.Users.GetByEmail(email);

        if (user is null)
        {
            throw new UserLoginException($"No user registered with email '{email}'.");
        }
        
        var passwordHash = string.Concat(SHA1.HashData(Encoding.ASCII.GetBytes(password))
            .Select(b => b.ToString("X2")));

        if (passwordHash != user.PasswordHash)
        {
            throw new UserLoginException("Typed incorrect password.");
        }
    }
}