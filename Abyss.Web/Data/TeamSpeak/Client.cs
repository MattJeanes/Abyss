namespace Abyss.Web.Data.TeamSpeak;

public struct Client
{
    public string Name { get; set; }
    public int ChannelId { get; set; }
    public int ConnectedSeconds { get; set; }
    public int IdleSeconds { get; set; }
}
