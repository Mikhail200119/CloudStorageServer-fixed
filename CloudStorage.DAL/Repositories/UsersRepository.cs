using CloudStorage.DAL.Entities;
using CloudStorage.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CloudStorage.DAL.Repositories;

public class UsersRepository : EfRepository<UserDbModel>, IUsersRepository
{
    public UsersRepository(DbContext context) : base(context)
    {
    }
    
    public void Create(UserDbModel userDbModel)
    {
        Table.Add(userDbModel);
    }

    public async Task<UserDbModel?> GetByEmail(string email)
    {
        var user = await Table.AsNoTracking().SingleOrDefaultAsync(u => u.Email == email);

        return user;
    }
}