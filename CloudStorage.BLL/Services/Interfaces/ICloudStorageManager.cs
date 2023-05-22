using CloudStorage.BLL.Models;

namespace CloudStorage.BLL.Services.Interfaces;

public interface ICloudStorageManager
{
    Task<IEnumerable<FileDescription>> CreateAsync(IEnumerable<FileCreateData> files);
    Task<(Stream Data, string ContentType, string DownloadName)> GetFileStreamAsync(int fileId);
    Task<(Stream Data, string ContentType)> GetThumbnailStreamAndContentTypeAsync(int thumbId);
    Task DeleteRangeAsync(IEnumerable<int> ids);
    Task<IEnumerable<FileDescription>> GetAllFilesAsync();
    Task<FileDescription?> GetFileDescriptionByIdAsync(int id);
    Task<IEnumerable<FileDescription>> SearchFilesAsync(FileSearchData fileSearchData);
    Task<IEnumerable<string>> GetArchiveFileNamesAsync(int fileId);
    Task<(string? name, string? contentType, Stream? data)> GetArchiveFileContent(int fileId, string archiveFilePath);
    Task<FileDescription> LoadFileFromZip(int zipFileId, string archiveFileName);
    Task RenameFileAsync(int id, string newName);
    Task<long> GetDiskUsageInBytes();
}