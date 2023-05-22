using System.Diagnostics;
using CloudStorage.BLL.Options;
using CloudStorage.BLL.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace CloudStorage.BLL.Services;

public class FilesDeduplicatorService : IFilesDeduplicator
{
    private readonly FileStorageOptions _fileStorageOptions;
    private readonly DeduplicationOptions _deduplicationOptions;

    public FilesDeduplicatorService(IOptions<FileStorageOptions> fileStorageOptions,
        IOptions<DeduplicationOptions> deduplicationOptions)
    {
        _deduplicationOptions = deduplicationOptions.Value;
        _fileStorageOptions = fileStorageOptions.Value;
    }

    public async Task DeduplicateData()
    {
        var str = "fqefqwefqwefiquwhef";
        
        var command = $"-c \"duperemove -dr {_fileStorageOptions.FilesDirectoryPath} --dedupe-options=partial\"";

        var processInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            Arguments = command
        };

        var process = new Process { StartInfo = processInfo };
        
        if (!process.Start())
        {
            throw new ApplicationException("Deduplication process error.");
        }
        
        /*await process.StandardInput.WriteLineAsync(_deduplicationOptions.AdminPassword);
        await process.StandardInput.FlushAsync();*/

        var errors = new List<string?>();

        while (!process.StandardError.EndOfStream)
        {
            errors.Add(await process.StandardError.ReadLineAsync());
        }

        var outputs = new List<string?>();
        
        while (!process.StandardOutput.EndOfStream)
        {
            outputs.Add(await process.StandardOutput.ReadLineAsync());
        }
        
        await process.WaitForExitAsync();

        var output = await process.StandardOutput.ReadLineAsync();
    }
}