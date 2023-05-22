using System.ComponentModel.DataAnnotations.Schema;
using CloudStorage.DAL.Entities.Interfaces;

namespace CloudStorage.DAL.Entities;

[Table("User")]
public class UserDbModel : IEntity
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
}