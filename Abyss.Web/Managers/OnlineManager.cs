﻿using Abyss.Web.Data.TeamSpeak;
using Abyss.Web.Helpers.Interfaces;
using Abyss.Web.Managers.Interfaces;

namespace Abyss.Web.Managers;

public class OnlineManager : IOnlineManager
{
    private readonly ITeamSpeakHelper _teamSpeakHelper;

    public OnlineManager(ITeamSpeakHelper teamSpeakHelper)
    {
        _teamSpeakHelper = teamSpeakHelper;
    }
    public async Task<List<Client>> GetClients()
    {
        return await _teamSpeakHelper.GetClients();
    }

    public async Task<List<Channel>> GetChannels()
    {
        return await _teamSpeakHelper.GetChannels();
    }
}
