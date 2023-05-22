using CloudStorage.DAL.Entities;
using CloudStorage.DAL.Repositories;
using CloudStorage.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CloudStorage.DAL;

public class CloudStorageUnitOfWork : DbContext, ICloudStorageUnitOfWork
{
    private IUsersRepository? _usersRepository;
    private IFileDescriptionRepository? _fileDescriptionRepository;
    private IThumbnailInfoRepository? _thumbnailInfoRepository;

    private DbSet<FileDescriptionDbModel> FileDescriptionTable { get; set; }
    private DbSet<UserDbModel> UsersTable { get; set; }

    public CloudStorageUnitOfWork(DbContextOptions<CloudStorageUnitOfWork> options) : base(options)
    {
    }

    public IUsersRepository Users => _usersRepository ??= new UsersRepository(this);
    public IFileDescriptionRepository FileDescription => _fileDescriptionRepository ??= new FileDescriptionRepository(this);
    public IThumbnailInfoRepository ThumbnailInfo => _thumbnailInfoRepository ??= new ThumbnailInfoRepository(this);

    public async Task SaveChangesAsync() => await base.SaveChangesAsync();
}