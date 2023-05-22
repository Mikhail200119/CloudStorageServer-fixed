using System.ComponentModel.DataAnnotations.Schema;
using CloudStorage.DAL.Entities.Interfaces;

namespace CloudStorage.DAL.Entities;

[Table("FileDescription")]
public class FileDescriptionDbModel : IEntity
{
    [Column("FileDescriptionId")]
    public int Id { get; set; }
    public string ProvidedName { get; set; }
    public string UniqueName { get; set; }
    public string ContentType { get; set; }
    public int SizeInBytes { get; set; }
    public DateTime CreatedDate { get; set; }
    public string ContentHash { get; set; }
    public string UploadedBy { get; set; }
    public string? Extension { get; set; }

    public ThumbnailInfoDbModel? ThumbnailInfo { get; set; }
}