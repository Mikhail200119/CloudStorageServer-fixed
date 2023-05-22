using System.Diagnostics;
using CloudStorage.BLL.Options;
using CloudStorage.BLL.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace CloudStorage.BLL.Services;

public class DeduplicationService : IDeduplicationService
{
    private readonly FileStorageOptions _fileStorageOptions;
    private readonly DeduplicationOptions _deduplicationOptions;
    
    public DeduplicationService(IOptions<FileStorageOptions> fileStorageOptions,
        IOptions<DeduplicationOptions> deduplicationOptions)
    {
        _deduplicationOptions = deduplicationOptions.Value;
        _fileStorageOptions = fileStorageOptions.Value;
    }
    
    public async Task Deduplicate()
    {
        var command = $"-c \"duperemove -dr {_fileStorageOptions.FilesDirectoryPath} --dedupe-options=partial\"";

        var processInfo = new ProcessStartInfo
        {
            FileName = _deduplicationOptions.CmdFileName,
            Arguments = command
        };

        var process = new Process { StartInfo = processInfo };
        
        if (!process.Start())
        {
            throw new ApplicationException($"Deduplication process error. Exit code: {process.ExitCode}.");
        }

        await process.WaitForExitAsync();

        process.Dispose();
    }

    public async Task<long> GetFilesDiskUsage(IEnumerable<string> fileNames)
    {
        if (!fileNames.Any())
        {
            throw new ArgumentException("File names count should be at least 1", nameof(fileNames));
        }
        
        var fileNamesString = string.Join(" ", fileNames
            .Select(name => Path.Combine(_fileStorageOptions.FilesDirectoryPath, name)));

        var processInfo = new ProcessStartInfo
        {
            FileName = _deduplicationOptions.CmdFileName,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            Arguments = $"-c \"echo {_deduplicationOptions.AdminPassword} | sudo -S compsize -b {fileNamesString}\""
        };

        var process = new Process { StartInfo = processInfo };
        
        if (!process.Start())
        {
            throw new ApplicationException($"Deduplication process error. Exit code: {process.ExitCode}.");
        }
        
        await process.WaitForExitAsync();

        var output = await process.StandardOutput.ReadToEndAsync();

        var lines = output.Split(Environment.NewLine);

        var rows = lines[2]
            .Split(" ")
            .Where(str => !string.IsNullOrWhiteSpace(str))
            .ToArray();

        var numberAsString = rows[2].Trim();
        var num = int.Parse(numberAsString);

        process.Dispose();

        return num;
    }
}