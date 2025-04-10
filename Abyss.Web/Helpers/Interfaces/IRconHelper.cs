using Abyss.Web.Data;
using Abyss.Web.Entities;

namespace Abyss.Web.Helpers.Interfaces;

public interface IRconHelper
{
    Task<string> ExecuteCommand(Server server, string command);
    
    bool SupportsServerType(ServerType serverType);
}