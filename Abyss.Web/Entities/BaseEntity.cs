using System.ComponentModel.DataAnnotations;

namespace Abyss.Web.Entities;

public class BaseEntity
{
    [Key]
    public int Id { get; set; }
}
