namespace CloudStorage.Api.Dtos.Request;

public class FileCreateRequest
{
    public IEnumerable<IFormFile> Files { get; set; }
}