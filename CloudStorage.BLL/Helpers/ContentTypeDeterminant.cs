namespace CloudStorage.BLL.Helpers;

public static class ContentTypeDeterminant
{
    private static readonly IEnumerable<string> VideoContentTypes;
    private static readonly IEnumerable<string> ImageContentTypes;

    static ContentTypeDeterminant()
    {
        VideoContentTypes = new List<string>
        {
            "video/x-flv",
            "video/mp4",
            "application/x-mpegURL",
            "video/MP2T",
            "video/3gpp",
            "video/quicktime",
            "video/x-msvideo",
            "video/x-ms-wmv"
        };

        ImageContentTypes = new List<string>
        {
            "image/apng",
            "image/avif",
            "image/gif",
            "image/jpeg",
            "image/png"
        };
    }

    public static bool IsVideo(string mimeContentType) => VideoContentTypes.Contains(mimeContentType);

    public static bool IsImage(string mimeContentType) => ImageContentTypes.Contains(mimeContentType);
}