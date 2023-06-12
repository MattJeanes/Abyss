using Abyss.Web.Data;
using Abyss.Web.Entities;

namespace Abyss.Web.Managers.Interfaces;

public interface IGPTManager
{
    Task<GPTResponse> Generate(GPTRequest message);
    Task<IList<GPTModelResponse>> GetModels();
}
