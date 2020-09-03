using Abyss.Web.Data;
using System.Threading.Tasks;

namespace Abyss.Web.Clients.Interfaces
{
    public interface IGPTClient
    {
        Task<GPTMessage> Generate(string message);
    }
}
