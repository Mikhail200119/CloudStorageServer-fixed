using CloudStorage.DAL.Entities;
using CloudStorage.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CloudStorage.DAL.Repositories;

public class FileDescriptionRepository : EfRepository<FileDescriptionDbModel>, IFileDescriptionRepository
{
    public FileDescriptionRepository(DbContext context) : base(context)
    {
    }

    public override async Task CreateAsync(FileDescriptionDbModel entity)
    {
        Context.Database.SetCommandTimeout(200);

        await base.CreateAsync(entity);
    }

    public async Task CreateRangeAsync(IEnumerable<FileDescriptionDbModel> filesDescriptions)
    {
        await Table.AddRangeAsync(filesDescriptions);
    }

    public async Task<bool> ContentHashesExistAsync(string userMail, params string[] contentHashes) =>
        await Table.AnyAsync(file => contentHashes.Contains(file.ContentHash) && file.UploadedBy == userMail);

    public async Task<FileDescriptionDbModel?> GetByIdAsync(int id)
    {
        return await Table
            .AsNoTracking()
            .Include(file => file.ThumbnailInfo)
            .SingleOrDefaultAsync(file => file.Id == id);
    }

    public void DeleteRange(IEnumerable<int> ids)
    {
        var entities = Table.AsNoTracking()
            .Where(item => ids.Contains(item.Id));

        Table.RemoveRange(entities);
    }

    public async Task<IEnumerable<FileDescriptionDbModel>> GetAllFilesAsync(string email, bool trackEntities = false)
    {
        var files = trackEntities ? Table : Table.AsNoTracking();

        return await files
            .Where(file => file.UploadedBy == email)
            .Include(file => file.ThumbnailInfo)
            .ToListAsync();
    }

    public IQueryable<FileDescriptionDbModel> GetAllFilesAsQueryable(string email)
    {
        return Table.AsNoTracking()
            .Where(f=>f.UploadedBy == email)
            .Include(file => file.ThumbnailInfo);
    }

    public async Task RenameFileAsync(int id, string newName)
    {
        var file = await Table.SingleOrDefaultAsync(file => file.Id == id);

        if (file is null)
        {
            return;
        }

        file.ProvidedName = newName;
    }

    public async Task<bool> ContentHashExist(string contentHash, string userEmail) => await Table.AnyAsync(file => file.ContentHash == contentHash && file.UploadedBy == userEmail);
    public async Task<bool> FileNameExist(string providedFileName, string userEmail) => await Table.AnyAsync(file => file.ProvidedName == providedFileName && file.UploadedBy == userEmail);

    public async Task<bool> FileNamesExist(string userMail, params string[] names) =>
        await Table.AnyAsync(file => names.Contains(file.ProvidedName) && file.UploadedBy == userMail);

    public async Task<IEnumerable<FileDescriptionDbModel>> GetAllFiles(Predicate<FileDescriptionDbModel> searchOption, string email, bool trackEntities = false)
    {
        return await Table.Where(file => searchOption(file))
            .ToListAsync();
    }
}