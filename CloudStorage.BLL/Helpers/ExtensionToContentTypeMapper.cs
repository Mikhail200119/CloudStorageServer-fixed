namespace CloudStorage.BLL.Helpers;

public class ExtensionToContentTypeMapper
{
    private const string ApplicationOctetSteam = "application/octet-stream";

    private static Dictionary<string, string> ContentTypes = new()
    {
        { "jpg", "image/jpg" },
        { "png", "image/png" },
        { "mp4", "video/mp4" },
        { "pdf", "application/pdf" },
        { "txt", "text/plain" }
    };

    public static string MapExtensionToContentType(string extension) =>
        !ContentTypes.ContainsKey(extension) ? "text/html" : ContentTypes[extension];
}