namespace CloudStorage.BLL.Services.Interfaces;

public interface IFilesDeduplicator
{
    Task DeduplicateData();
}