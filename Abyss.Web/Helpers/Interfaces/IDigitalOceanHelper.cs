using Abyss.Web.Entities;
using Abyss.Web.Logging;
using DigitalOcean.API.Models.Responses;
using System.Threading.Tasks;

namespace Abyss.Web.Helpers.Interfaces
{
    public interface IDigitalOceanHelper
    {
        Task<Droplet> CreateDropletFromServer(Server server, TaskLogger logger);
        Task<Droplet> GetDroplet(int value);
        Task Shutdown(int dropletId);
        Task<Image> Snapshot(int dropletId, string snapshotName);
        Task DeleteSnapshot(int id);
        Task DeleteDroplet(int id);
    }
}
