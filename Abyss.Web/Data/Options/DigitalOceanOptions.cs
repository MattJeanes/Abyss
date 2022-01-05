using System.Diagnostics.CodeAnalysis;

namespace Abyss.Web.Data.Options;

public class DigitalOceanOptions
{
    [NotNull]
    public string? ApiKey { get; set; }

    public int ActionTimeout { get; set; }

    public int TimeBetweenChecks { get; set; }

    public int TimeBetweenActions { get; set; }

    public int SshId { get; set; }
}
