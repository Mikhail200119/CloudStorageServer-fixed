using CloudStorage.BLL.Services.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace CloudStorage.BLL.Services;

internal static class FileStorageServiceExtensions
{ 
    public static async Task<Stream> GetCompressedImage(this IFileStorageService fileStorageService, string fileName)
    {
        var imageStream = await fileStorageService.GetStreamAsync(fileName);
        var image = await Image.LoadAsync(imageStream);
        var memoryStream = new MemoryStream();
        await image.SaveAsync(memoryStream, new JpegEncoder { Quality = 1 });

        return memoryStream;
    }
}