using Abyss.Web.Data;
using Abyss.Web.Entities;
using Abyss.Web.Managers.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abyss.Web.Controllers
{
    [Route("api/gpt")]
    public class GPTController
    {
        private readonly IGPTManager _gptManager;

        public GPTController(IGPTManager gptManager)
        {
            _gptManager = gptManager;
        }

        [HttpGet("models")]
        public async Task<IList<GPTModel>> GetModels()
        {
            return await _gptManager.GetModels();
        }

        [HttpPost]
        public async Task<GPTResponse> Generate([FromBody] GPTRequest message)
        {
            return await _gptManager.Generate(message);
        }
    }
}
