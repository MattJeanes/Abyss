namespace Abyss.Web.Data.GMod;

public struct ChangeRankDTO
{
    public string SteamId64 { get; set; }
    public string Rank { get; set; }
    public bool CanDemote { get; set; }
    public string MaxRankForDemote { get; set; }
}
