using System.Diagnostics.CodeAnalysis;

namespace Abyss.Web.Data.Options;

public class JwtOptions
{
    [NotNull]
    public string? Key { get; set; }

    [NotNull]
    public string? Issuer { get; set; }
}
