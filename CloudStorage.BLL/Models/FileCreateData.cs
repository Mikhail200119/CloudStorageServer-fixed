namespace CloudStorage.BLL.Models;

public class FileCreateData
{
    public string Name { get; set; }

    public Stream Content { get; set; }

    public string ContentType { get; set; }
}