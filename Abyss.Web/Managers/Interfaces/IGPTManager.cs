using Abyss.Web.Data;
using Abyss.Web.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abyss.Web.Managers.Interfaces
{
    public interface IGPTManager
    {
        Task<GPTResponse> Generate(GPTRequest message);
        Task<IList<GPTModel>> GetModels();
    }
}
