namespace CloudStorage.BLL.Models;

public class FileFolderUpdateData
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int? ParentFolderId { get; set; }
    public IEnumerable<FileUpdateData> Files { get; set; }
}