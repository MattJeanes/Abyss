using Abyss.Web.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Abyss.Web.Entities;

[Table("UserAuthentications")]
[PrimaryKey(nameof(UserId), nameof(SchemeType))]
public class UserAuthentication
{
    [ForeignKey(nameof(User))]
    public int UserId { get; set; }

    [JsonIgnore]
    public User User { get; set; }

    [Column("SchemeTypeId")]
    public AuthSchemeType SchemeType { get; set; }

    public string Identifier { get; set; }
}
