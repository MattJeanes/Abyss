using Abyss.Web.Clients.Interfaces;
using Abyss.Web.Data;
using Abyss.Web.Managers.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Abyss.Web.Managers
{
    public class GPTManager : IGPTManager
    {
        private readonly ILogger<GPTManager> _logger;
        private readonly IGPTClient _gptClient;

        public GPTManager(ILogger<GPTManager> logger, IGPTClient gptClient)
        {
            _logger = logger;
            _gptClient = gptClient;
        }

        public async Task<GPTMessage> Generate(string message)
        {
            var response = await _gptClient.Generate(message);
            _logger.LogInformation($"GPT: {response}");
            return response;
        }
    }
}
