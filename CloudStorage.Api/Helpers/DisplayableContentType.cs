using Microsoft.AspNetCore.StaticFiles;

namespace CloudStorage.Api.Helpers;

public static class DisplayableContentType
{
    public static bool IsDisplayable(string contentType)
    {
        return new FileExtensionContentTypeProvider().Mappings
            .Where(pair => pair.Value.Contains("video")
                           || pair.Value.Contains("image")
                           || pair.Value.Contains("archive")
                           || pair.Value.Contains("pdf")
                           || pair.Value.Contains("word")
                           || pair.Value.Contains("text"))
            .Any(pair => pair.Value == contentType);
    }
}