using Abyss.Web.Data;
using System.Threading.Tasks;

namespace Abyss.Web.Managers.Interfaces
{
    public interface IGPTManager
    {
        Task<GPTMessage> Generate(string message);
    }
}
