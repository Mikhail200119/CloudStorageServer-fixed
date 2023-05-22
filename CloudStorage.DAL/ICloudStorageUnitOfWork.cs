using CloudStorage.DAL.Repositories.Interfaces;

namespace CloudStorage.DAL;

public interface ICloudStorageUnitOfWork
{
    IUsersRepository Users { get; }
    IFileDescriptionRepository FileDescription { get; }
    IThumbnailInfoRepository ThumbnailInfo { get; }

    Task SaveChangesAsync();
}