using CloudStorage.DAL.Entities;

namespace CloudStorage.DAL.Repositories.Interfaces;

public interface IThumbnailInfoRepository
{
    Task<ThumbnailInfoDbModel?> GetByIdAsync(int id);
}