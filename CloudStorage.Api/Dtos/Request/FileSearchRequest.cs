namespace CloudStorage.Api.Dtos.Request;

public class FileSearchRequest
{
    public string? Name { get; set; }
    public string? Extension { get; set; }
    public int? SizeInBytes { get; set; }
    public DateTime? CreationDate { get; set; }
}