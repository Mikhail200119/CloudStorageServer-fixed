using Microsoft.EntityFrameworkCore;

namespace CloudStorage.DAL.Repositories.Interfaces;

public interface IDatabaseExecutable<TEntity> where TEntity : class
{
    Task<TEntity> ExecuteAsync(Func<DbSet<TEntity>, TEntity> func);
}