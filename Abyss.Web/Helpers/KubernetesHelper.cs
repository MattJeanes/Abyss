using Abyss.Web.Entities;
using Abyss.Web.Helpers.Interfaces;
using Abyss.Web.Logging;

namespace Abyss.Web.Helpers;

public class KubernetesHelper() : IKubernetesHelper
{
    public Task StartServer(Server server, TaskLogger logger)
    {
        throw new NotImplementedException("Kubernetes start server not implemented yet");
    }

    public Task StopServer(Server server, TaskLogger logger)
    {
        throw new NotImplementedException("Kubernetes stop server not implemented yet");
    }

    public Task RestartServer(Server server, TaskLogger logger)
    {
        throw new NotImplementedException("Kubernetes restart server not implemented yet");
    }

    public Task<string> GetServerIpAddress(Server server)
    {
        throw new NotImplementedException("Kubernetes get server IP address not implemented yet");
    }
}