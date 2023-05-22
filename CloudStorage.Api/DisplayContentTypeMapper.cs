namespace CloudStorage.Api;

public class DisplayContentTypeMapper : IDisplayContentTypeMapper
{
    private readonly IEnumerable<string> DisplayedContentTypes = new[]
    {
        ""
    };
    
    public string Map(string contentType)
    {
        if (contentType.Contains("text"))
        {
            return "text/plain";
        }

        return "text/html";
    }
}