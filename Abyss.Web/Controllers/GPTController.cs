using Abyss.Web.Data;
using Abyss.Web.Helpers;
using Abyss.Web.Managers.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Abyss.Web.Controllers
{
    [AuthorizePermission(Permissions.GPT)]
    [Route("api/gpt")]
    public class GPTController
    {
        private readonly IGPTManager _gptManager;

        public GPTController(IGPTManager gptManager)
        {
            _gptManager = gptManager;
        }

        [HttpPost]
        public async Task<GPTMessage> Generate([FromBody] GPTMessage message)
        {
            return await _gptManager.Generate(message);
        }
    }
}
