namespace CloudStorage.BLL.Services.Interfaces;

public interface IFileStorageService
{
    Task UploadRangeAsync(IEnumerable<(string FileName, Stream Content)> files);
    void Delete(string fileName);
    Task DeleteRangeAsync(params string[] fileNames);
    Task<Stream> GetStreamAsync(string fileName);
    Task CreateVideoThumbnailAsync(string existingFileName, string thumbName);
    Task<(Stream data, string entryName)> ExtractZipEntry(string zipFileName, string newFileName, string extractedFileName);
}