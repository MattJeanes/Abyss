﻿using Abyss.Web.Data;
using Abyss.Web.Helpers.Interfaces;
using Abyss.Web.Managers.Interfaces;

namespace Abyss.Web.Managers;

public class WhoSaidManager(IWhoSaidHelper whoSaidHelper, ILogger<WhoSaidManager> logger) : IWhoSaidManager
{
    private readonly IWhoSaidHelper _whoSaidHelper = whoSaidHelper;
    private readonly ILogger<WhoSaidManager> _logger = logger;

    public async Task<WhoSaid> WhoSaid(string message)
    {
        var whoSaid = await _whoSaidHelper.WhoSaid(message);
        _logger.LogInformation($"{whoSaid.Name}: {whoSaid.Message}");
        return whoSaid;
    }
}
