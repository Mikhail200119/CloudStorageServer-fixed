namespace CloudStorage.BLL.Services.Interfaces;

public interface IDeduplicationService
{
    Task Deduplicate();
    Task<long> GetFilesDiskUsage(IEnumerable<string> fileNames);
}