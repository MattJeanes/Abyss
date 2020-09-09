using Abyss.Web.Data;
using System.Threading.Tasks;

namespace Abyss.Web.Clients.Interfaces
{
    public interface IGPTClient
    {
        Task<GPTResponse> Generate(string model, string message);
    }
}
