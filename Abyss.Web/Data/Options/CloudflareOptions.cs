using System.Diagnostics.CodeAnalysis;

namespace Abyss.Web.Data.Options;

public class CloudflareOptions
{
    [NotNull]
    public string? BaseUrl { get; set; }

    [NotNull]
    public string? Email { get; set; }

    [NotNull]
    public string? ApiKey { get; set; }

    [NotNull]
    public string? Zone { get; set; }
}
