using System.Diagnostics.CodeAnalysis;

namespace Abyss.Web.Data.Options;

public class PushoverOptions
{
    [NotNull]
    public string? ApiKey { get; set; }

    [NotNull]
    public string? UserKey { get; set; }
}
