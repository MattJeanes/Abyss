using Abyss.Web.Clients.Interfaces;
using Abyss.Web.Data;
using Abyss.Web.Entities;
using Abyss.Web.Helpers.Interfaces;
using Abyss.Web.Managers.Interfaces;
using Abyss.Web.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Abyss.Web.Managers;

public class GPTManager : IGPTManager
{
    private readonly ILogger<GPTManager> _logger;
    private readonly IGPTClient _gptClient;
    private readonly IGPTModelRepository _gptModelRepository;
    private readonly IUserHelper _userHelper;

    public GPTManager(
        ILogger<GPTManager> logger,
        IGPTClient gptClient,
        IGPTModelRepository gptModelRepository,
        IUserHelper userHelper
        )
    {
        _logger = logger;
        _gptClient = gptClient;
        _gptModelRepository = gptModelRepository;
        _userHelper = userHelper;
    }

    public async Task<GPTResponse> Generate(GPTRequest message)
    {
        var model = await _gptModelRepository.GetById(message.ModelId);
        if (model?.Identifier == null)
        {
            throw new ArgumentException(nameof(message.ModelId), "Invalid model");
        }
        if (model.Permission != null)
        {
            if (model.Permission?.Identifier == null || !_userHelper.HasPermission(model.Permission.Identifier))
            {
                throw new Exception("User does not have permission to use this model");
            }
        }
        var response = await _gptClient.Generate(model.Identifier, message.Text, message.Temperature, message.TopP);
        _logger.LogInformation($"GPT: {response.Text}");
        return response;
    }

    public async Task<IList<GPTModel>> GetModels()
    {
        var permissions = await _userHelper.GetPermissions();
        var models = await _gptModelRepository.GetAll().Include(x => x.Permission).ToListAsync();
        return models
            .Where(x => x.Permission == null || permissions.Any(y => y.Id.Equals(x.Permission.Id)))
            .OrderBy(x => x.Name)
            .ToList();
    }
}
