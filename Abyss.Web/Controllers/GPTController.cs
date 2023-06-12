using Abyss.Web.Data;
using Abyss.Web.Entities;
using Abyss.Web.Helpers.Interfaces;
using Abyss.Web.Managers.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Abyss.Web.Controllers;

[Route("api/gpt")]
public class GPTController : BaseController
{
    private readonly IGPTManager _gptManager;

    public GPTController(IGPTManager gptManager, IUserHelper userHelper) : base(userHelper)
    {
        _gptManager = gptManager;
    }

    [HttpGet("models")]
    public async Task<IList<GPTModelResponse>> GetModels()
    {
        return await _gptManager.GetModels();
    }

    [HttpPost]
    public async Task<GPTResponse> Generate([FromBody] GPTRequest message)
    {
        return await _gptManager.Generate(message);
    }
}
