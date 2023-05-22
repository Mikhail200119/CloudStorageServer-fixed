using System.ComponentModel.DataAnnotations.Schema;
using CloudStorage.DAL.Entities.Interfaces;

namespace CloudStorage.DAL.Entities;

[Table(("ThumbnailInfo"))]
public class ThumbnailInfoDbModel : IEntity
{
    [Column("ThumbnailId")]
    public int Id { get; set; }
    public string? UniqueName { get; set; }

    public int FileDescriptionId { get; set; }
    public FileDescriptionDbModel FileDescription { get; set; }
}