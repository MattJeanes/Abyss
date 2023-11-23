using Abyss.Web.Data;
using Abyss.Web.Entities;
using Abyss.Web.Helpers.Interfaces;
using Abyss.Web.Managers.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Abyss.Web.Controllers;

[Route("api/gpt")]
public class GPTController(IGPTManager gptManager, IUserHelper userHelper) : BaseController(userHelper)
{
    private readonly IGPTManager _gptManager = gptManager;

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
