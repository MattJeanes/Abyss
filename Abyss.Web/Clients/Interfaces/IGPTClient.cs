using Abyss.Web.Data;

namespace Abyss.Web.Clients.Interfaces;

public interface IGPTClient
{
    Task<GPTResponse> Generate(string model, string message);
}
