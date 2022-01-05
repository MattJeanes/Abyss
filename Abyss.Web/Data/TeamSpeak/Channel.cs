namespace Abyss.Web.Data.TeamSpeak;

public struct Channel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int? ParentId { get; set; }
}
