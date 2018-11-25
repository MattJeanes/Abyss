using Abyss.Web.Entities;
using DigitalOcean.API.Models.Responses;
using System.Threading.Tasks;

namespace Abyss.Web.Managers.Interfaces
{
    public interface IDigitalOceanHelper
    {
        Task CreateDropletFromServerAndDeleteSnapshot(Server server);
        Task DeleteAndSnapshotDroplet(Server server);
    }
}
