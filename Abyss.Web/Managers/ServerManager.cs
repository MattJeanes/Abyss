using Abyss.Web.Data.Options;
using Abyss.Web.Entities;
using Abyss.Web.Managers.Interfaces;
using Abyss.Web.Repositories.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abyss.Web.Managers
{
    public class ServerManager : IServerManager
    {
        private readonly IDigitalOceanHelper _digitalOceanHelper;
        private readonly IRepository<Server> _serverRepository;
        private readonly ServerOptions _options;

        public ServerManager(IDigitalOceanHelper digitalOceanHelper, IRepository<Server> serverRepository, IOptions<ServerOptions> options)
        {
            _digitalOceanHelper = digitalOceanHelper;
            _serverRepository = serverRepository;
            _options = options.Value;
        }

        public async Task<List<Server>> GetServers()
        {
            return await _serverRepository.GetAll().ToListAsync();
        }

        public async Task Start(string serverId)
        {
            var server = await _serverRepository.GetById(serverId);
            if (server == null) { throw new Exception($"Server id {server} not found"); }

            await _digitalOceanHelper.CreateDropletFromServerAndDeleteSnapshot(server);
        }

        public async Task Stop(string serverId)
        {
            var server = await _serverRepository.GetById(serverId);
            if (server == null) { throw new Exception($"Server id {server} not found"); }

            await _digitalOceanHelper.DeleteAndSnapshotDroplet(server);
        }
    }
}
