namespace Abyss.Web.Data.Options;

public class DiscordOptions
{
    public string Token { get; set; }

    public string CommandPrefix { get; set; }

    public string MemberRankId { get; set; }

    public string MemberRankName { get; set; }

    public string GuestRankId { get; set; }

    public ulong? GuildId { get; set; }
}
