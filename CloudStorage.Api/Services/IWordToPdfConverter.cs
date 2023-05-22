namespace CloudStorage.Api.Services;

public interface IWordToPdfConverter
{
    Task<Stream> GetPdfFromWordAsync(int fileId);
    Task<Stream> GetPdfFromWordAsync(Stream wordStream, string wordFileName, string wordExtension);
}