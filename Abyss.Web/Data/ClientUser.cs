namespace Abyss.Web.Data;

public class ClientUser
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Dictionary<int, string> Authentication { get; set; }
    public int? RoleId { get; set; }
    public List<string> Permissions { get; set; }
}
