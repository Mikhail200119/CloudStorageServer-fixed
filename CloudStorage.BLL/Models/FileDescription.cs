namespace CloudStorage.BLL.Models;

public class FileDescription
{
    public int Id { get; set; }

    public string ProvidedName { get; set; }

    public string UploadedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string ContentType { get; set; }

    public string ContentHash { get; set; }

    public int SizeInBytes { get; set; }
    public int ThumbnailId { get; set; }
    public string Extension { get; set; }
}