
using Abyss.Web.Entities;
using Abyss.Web.Logging;

namespace Abyss.Web.Helpers.Interfaces;

public interface IOvhHelper
{
    Task StartServer(Server server, TaskLogger logger);
    Task StopServer(Server server, TaskLogger logger);
    Task RestartServer(Server server, TaskLogger logger, OvhHelper.ServerRebootType type = OvhHelper.ServerRebootType.Soft);
    Task<string> GetServerIpAddress(Server server);
}