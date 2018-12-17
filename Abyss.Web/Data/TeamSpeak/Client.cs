namespace Abyss.Web.Data.TeamSpeak
{
    public class Client
    {
        public string Name { get; set; }
        public int ChannelId { get; set; }
        public int ConnectedSeconds { get; set; }
        public int IdleSeconds { get; internal set; }
    }
}
