using CloudStorage.DAL.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CloudStorage.DAL.Repositories;

public abstract class EfRepository<TEntity>  where TEntity : class, IEntity, new()
{
    protected readonly DbContext Context;
    protected readonly DbSet<TEntity> Table;

    protected EfRepository(DbContext context)
    {
        Context = context;
        Table = Context.Set<TEntity>();
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(bool trackEntities = false)
    {
        var query = trackEntities == false ? Table.AsNoTracking() : Table;

        return await query.ToListAsync();
    }

    public virtual async Task CreateAsync(TEntity entity) => await Table.AddAsync(entity);

    public virtual void Update(TEntity entity) => Table.Update(entity);

    public virtual void Delete(TEntity entity) => Table.Remove(entity);

    public virtual void Delete(int id)
    {
        var entity = Context.ChangeTracker
            .Entries<TEntity>()
            .FirstOrDefault(entry => entry.Entity.Id == id)
            ?.Entity ?? new TEntity { Id = id };

        Table.Remove(entity);
    }
}