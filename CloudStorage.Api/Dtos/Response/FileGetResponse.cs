namespace CloudStorage.Api.Dtos.Response;

public class FileGetResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string FileSrc { get; set; }
    public int ThumbnailId { get; set; }
    public string ContentType { get; set; }
    public string ThumbnailSrc { get; set; }
    public DateTime CreatedDate { get; set; }
    public string Extension { get; set; }
    public int SizeInBytes{ get; set; }
}