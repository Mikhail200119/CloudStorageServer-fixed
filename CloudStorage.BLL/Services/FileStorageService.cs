using System.IO.Compression;
using CloudStorage.BLL.Options;
using CloudStorage.BLL.Services.Interfaces;
using Microsoft.Extensions.Options;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace CloudStorage.BLL.Services;

public class FileStorageService : IFileStorageService
{
    private readonly FileStorageOptions _storageOptions;

    public FileStorageService(IOptions<FileStorageOptions> fileStorageOptions)
    {
        _storageOptions = fileStorageOptions.Value;
    }

    public async Task UploadStreamAsync(string fileName, Stream data)
    {
        var filePath = Path.Combine(_storageOptions.FilesDirectoryPath, fileName);

        await using var fileStream = File.Create(filePath);
        data.Seek(0, SeekOrigin.Begin);
        await data.CopyToAsync(fileStream);
    }

    public async Task UploadRangeAsync(IEnumerable<(string FileName, Stream Content)> files)
    {
        var uploadFileTasks = files.Select(file =>
        {
            var (fileName, content) = file;
            var filePath = Path.Combine(_storageOptions.FilesDirectoryPath, fileName);

            return Task.Run(async () =>
            {
                await using var fileStream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None);

                if (content.CanSeek)
                {
                    content.Seek(0, SeekOrigin.Begin);
                }
                
                await content.CopyToAsync(fileStream);
            });
        });

        await Task
            .WhenAll(uploadFileTasks)
            .ConfigureAwait(false);
    }

    public void Delete(string fileName)
    {
        var filePath = Path.Combine(_storageOptions.FilesDirectoryPath, fileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    public async Task DeleteRangeAsync(params string[] fileNames)
    {
        foreach (var fileName in fileNames)
        {
            await Task.Run(() =>
            {
                if (fileName is null)
                {
                    return;
                }
            
                var filePath = Path.Combine(_storageOptions.FilesDirectoryPath, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            });
        }
    }

    public async Task<Stream> GetStreamAsync(string fileName)
    {
        var filePath = Path.Combine(_storageOptions.FilesDirectoryPath, fileName);

        var file = File.Open(filePath, new FileStreamOptions
        {
            Mode = FileMode.Open,
            Access = FileAccess.Read,
            Share = FileShare.Delete | FileShare.Read
        });
        
        return await Task.FromResult(file);
    }

    public async Task CreateVideoThumbnailAsync(string existingFileName, string thumbName)
    {
        var filePath = Path.Combine(_storageOptions.FilesDirectoryPath, existingFileName);
        var outputFilePath = Path.Combine(_storageOptions.FilesDirectoryPath, $"{thumbName}.jpeg");

        var conversion =
            await FFmpeg.Conversions.FromSnippet.Snapshot(filePath, outputFilePath, TimeSpan.FromSeconds(0));
        await conversion.Start();

        await using var compressedImage = await this.GetCompressedImage($"{thumbName}.jpeg");
        compressedImage.Seek(0, SeekOrigin.Begin);
        Delete(outputFilePath);
        var finalOutputPath = Path.Combine(_storageOptions.FilesDirectoryPath, thumbName);
        await using var finalThumbnail = File.Create(finalOutputPath);
        await compressedImage.CopyToAsync(finalThumbnail);
    }

    public async Task<(Stream data, string entryName)> ExtractZipEntry(string zipFileName, string newFileName, string extractedFileName)
    {
        await using var archive = await GetStreamAsync(zipFileName);
        var zipArchive = new ZipArchive(archive);
        var entry = zipArchive.Entries.SingleOrDefault(e => e.FullName == extractedFileName);
        var filePath = Path.Combine(_storageOptions.FilesDirectoryPath, newFileName);
        entry?.ExtractToFile(filePath);

        return (await GetStreamAsync(newFileName), entry?.Name)!;
    }
}