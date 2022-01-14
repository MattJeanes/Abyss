using Abyss.Web.Entities;
using Abyss.Web.Logging;
using DigitalOcean.API.Models.Responses;

namespace Abyss.Web.Helpers.Interfaces;

public interface IDigitalOceanHelper
{
    Task<Droplet> CreateDropletFromServer(Server server, TaskLogger logger);
    Task<Droplet> GetDroplet(long value);
    Task Shutdown(long dropletId);
    Task<Image?> Snapshot(long dropletId, string snapshotName);
    Task DeleteSnapshot(long id);
    Task DeleteDroplet(long id);
    Task Restart(long dropletId);
}
