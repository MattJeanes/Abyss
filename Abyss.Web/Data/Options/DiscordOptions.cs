using System.Diagnostics.CodeAnalysis;

namespace Abyss.Web.Data.Options;

public class DiscordOptions
{
    [NotNull]
    public string? Token { get; set; }

    [NotNull]
    public string? CommandPrefix { get; set; }

    [NotNull]
    public string? MemberRankId { get; set; }

    [NotNull]
    public string? MemberRankName { get; set; }

    [NotNull]
    public string? GuestRankId { get; set; }

    [NotNull]
    public string? AddonsMessage { get; set; }
}
