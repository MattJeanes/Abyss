using Abyss.Web.Entities;
using Abyss.Web.Logging;
using Microsoft.Azure.Management.Compute.Fluent;

namespace Abyss.Web.Helpers.Interfaces;

public interface IAzureHelper
{
    Task<IVirtualMachine> StartServer(Server server, TaskLogger logger);
    Task<IVirtualMachine> StopServer(Server server, TaskLogger logger);
}
