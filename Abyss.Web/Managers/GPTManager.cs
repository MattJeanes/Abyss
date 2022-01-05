using Abyss.Web.Clients.Interfaces;
using Abyss.Web.Data;
using Abyss.Web.Entities;
using Abyss.Web.Helpers.Interfaces;
using Abyss.Web.Managers.Interfaces;
using Abyss.Web.Repositories.Interfaces;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Abyss.Web.Managers;

public class GPTManager : IGPTManager
{
    private readonly ILogger<GPTManager> _logger;
    private readonly IGPTClient _gptClient;
    private readonly IRepository<GPTModel> _gptModelRepository;
    private readonly IUserHelper _userHelper;
    private readonly IRepository<Permission> _permissionRepository;

    public GPTManager(
        ILogger<GPTManager> logger,
        IGPTClient gptClient,
        IRepository<GPTModel> gptModelRepository,
        IUserHelper userHelper,
        IRepository<Permission> permissionRepository
        )
    {
        _logger = logger;
        _gptClient = gptClient;
        _gptModelRepository = gptModelRepository;
        _userHelper = userHelper;
        _permissionRepository = permissionRepository;
    }

    public async Task<GPTResponse> Generate(GPTRequest message)
    {
        var model = await _gptModelRepository.GetById(message.ModelId);
        if (model?.Identifier == null)
        {
            throw new ArgumentException(nameof(message.ModelId), "Invalid model");
        }
        if (model.Permission.HasValue)
        {
            var permission = await _permissionRepository.GetById(model.Permission.Value);
            if (permission?.Identifier == null || !_userHelper.HasPermission(permission.Identifier))
            {
                throw new Exception("User does not have permission to use this model");
            }
        }
        var response = await _gptClient.Generate(model.Identifier, message.Text);
        _logger.LogInformation($"GPT: {response.Text}");
        return response;
    }

    public async Task<IList<GPTModel>> GetModels()
    {
        var permissions = await _userHelper.GetPermissions();
        var models = await _gptModelRepository.GetAll().ToListAsync();
        return models
            .Where(x => !x.Permission.HasValue || permissions.Select(x => x.Id).Any(y => y.Equals(x.Permission.Value)))
            .ToList();
    }
}
