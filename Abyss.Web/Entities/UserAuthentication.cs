using Abyss.Web.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abyss.Web.Entities;

[PrimaryKey(nameof(UserId), nameof(SchemeType))]
public class UserAuthentication
{
    [ForeignKey(nameof(User))]
    public int UserId { get; set; }

    public User User { get; set; }

    [Column("SchemeTypeId")]
    public AuthSchemeType SchemeType { get; set; }

    public string Identifier { get; set; }
}
