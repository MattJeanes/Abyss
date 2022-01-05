using System.Diagnostics.CodeAnalysis;

namespace Abyss.Web.Data.Options;

public class TumblrOptions
{
    [NotNull]
    public string? BlogName { get; set; }

    public int CacheMinutes { get; set; }
}
