namespace CloudStorage.BLL.Models;

public class FileFolderCreateData
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int? ParentFolderId { get; set; }
    public IEnumerable<FileCreateData> Files { get; set; }
}