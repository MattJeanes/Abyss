namespace Abyss.Web.Data.Options;

public class WebhookRelayOptions
{
    public class WebhookRelay
    {
        public string Key { get; set; }
        public List<string> Urls { get; set; }
    }

    public Dictionary<string, WebhookRelay> Relays { get; set; }
}
