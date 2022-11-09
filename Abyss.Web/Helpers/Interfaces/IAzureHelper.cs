using Abyss.Web.Entities;
using Abyss.Web.Logging;
using Azure.ResourceManager.Compute;

namespace Abyss.Web.Helpers.Interfaces;

public interface IAzureHelper
{
    Task<VirtualMachineResource> StartServer(Server server, TaskLogger logger);
    Task<VirtualMachineResource> StopServer(Server server, TaskLogger logger);
    Task<VirtualMachineResource> RestartServer(Server server, TaskLogger logger);
    Task<string> GetServerIpAddress(Server server);
}
