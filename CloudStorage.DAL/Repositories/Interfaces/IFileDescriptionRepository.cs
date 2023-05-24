using CloudStorage.DAL.Entities;

namespace CloudStorage.DAL.Repositories.Interfaces;

public interface IFileDescriptionRepository
{
    Task CreateAsync(FileDescriptionDbModel fileDescription);
    Task CreateRangeAsync(IEnumerable<FileDescriptionDbModel> filesDescriptions);
    Task<FileDescriptionDbModel?> GetByIdAsync(int id);
    void Update(FileDescriptionDbModel fileDescription);
    void Delete(int id);
    void DeleteRange(IEnumerable<int> ids);
    Task<IEnumerable<FileDescriptionDbModel>> GetAllFilesAsync(string email, bool trackEntities = false);
    IQueryable<FileDescriptionDbModel> GetAllFilesAsQueryable(string email);
    Task RenameFileAsync(int id, string newName);
}