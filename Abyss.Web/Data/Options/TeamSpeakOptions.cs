using System.Diagnostics.CodeAnalysis;

namespace Abyss.Web.Data.Options;

public class TeamSpeakOptions
{
    public int UpdateIntervalSeconds { get; set; }

    [NotNull]
    public string? Host { get; set; }

    public int ServerId { get; set; }

    [NotNull]
    public string? ClientName { get; set; }
}
