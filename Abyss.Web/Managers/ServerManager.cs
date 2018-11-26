using Abyss.Web.Data;
using Abyss.Web.Data.Options;
using Abyss.Web.Entities;
using Abyss.Web.Helpers.Interfaces;
using Abyss.Web.Managers.Interfaces;
using Abyss.Web.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Abyss.Web.Managers
{
    public class ServerManager : IServerManager
    {
        private readonly IDigitalOceanHelper _digitalOceanHelper;
        private readonly IRepository<Server> _serverRepository;
        private readonly CloudflareOptions _options;
        private readonly ICloudflareHelper _cloudflareHelper;
        private readonly ILogger<ServerManager> _logger;

        public ServerManager(
            IDigitalOceanHelper digitalOceanHelper,
            IRepository<Server> serverRepository,
            IOptions<CloudflareOptions> options,
            ICloudflareHelper cloudflareHelper,
            ILogger<ServerManager> logger
            )
        {
            _digitalOceanHelper = digitalOceanHelper;
            _serverRepository = serverRepository;
            _options = options.Value;
            _cloudflareHelper = cloudflareHelper;
            _logger = logger;
        }

        public async Task<List<Server>> GetServers()
        {
            return await _serverRepository.GetAll().ToListAsync();
        }

        public async Task Start(string serverId)
        {
            Server server = null;
            try
            {
                server = await _serverRepository.GetById(serverId);
                if (server == null) { throw new Exception($"Server id {server} not found"); }
                if (server.StatusId != ServerStatus.Inactive) { throw new Exception("Cannot create server that is active"); }
                if (!server.SnapshotId.HasValue) { throw new Exception("Server has no snapshot id"); }
                server.StatusId = ServerStatus.Activating;
                await _serverRepository.Update(server);

                _logger.LogInformation($"Creating droplet from server id {server.Id}");
                var droplet = await _digitalOceanHelper.CreateDropletFromServer(server);
                _logger.LogInformation($"Created droplet from server id {server.Id}");

                _logger.LogInformation($"Setting DNS record for {server.DNSRecord} to {server.IPAddress}");
                var dnsRecord = await _cloudflareHelper.GetDNSRecord(server.DNSRecord);
                dnsRecord.Content = server.IPAddress;
                await _cloudflareHelper.UpdateDNSRecord(dnsRecord);
                _logger.LogInformation($"Set DNS record for {server.DNSRecord} to {server.IPAddress}");

                server.DropletId = droplet.Id;
                server.StatusId = ServerStatus.Active;
                server.IPAddress = droplet.Networks.v4.FirstOrDefault()?.IpAddress ?? throw new Exception("Droplet has no IPv4 address");
                await _serverRepository.Update(server);
                _logger.LogInformation($"Successfully started server {server.Id}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to start server {server?.Id.ToString() ?? "N/A"}");
            }
            finally
            {
                if (server?.StatusId == ServerStatus.Activating)
                {
                    server.StatusId = ServerStatus.Inactive;
                    await _serverRepository.Update(server);
                }
            }

        }

        public async Task Stop(string serverId)
        {
            Server server = null;
            try
            {
                server = await _serverRepository.GetById(serverId);
                if (server == null) { throw new Exception($"Server id {server} not found"); }

                if (server.StatusId != ServerStatus.Active) { throw new Exception("Cannot delete server that is not active"); }
                if (!server.DropletId.HasValue) { throw new Exception("Server has no droplet id"); }
                var droplet = await _digitalOceanHelper.GetDroplet(server.DropletId.Value);
                if (droplet == null) { throw new Exception($"Droplet id {server.DropletId} not valid"); }
                var dropletName = $"{droplet.Name} (id {droplet.Id})";
                _logger.LogDebug($"Found droplet {dropletName}");
                if (!new[] { DigitalOceanEnums.DropletStatus.Active, DigitalOceanEnums.DropletStatus.Off }.Contains(droplet.Status)) { throw new Exception($"Server not in expected state, is in '{droplet.Status}'"); }
                _logger.LogInformation($"Stopping server {server.Id}");
                server.StatusId = ServerStatus.Deactivating;
                await _serverRepository.Update(server);

                if (droplet.Status == DigitalOceanEnums.DropletStatus.Active)
                {
                    _logger.LogInformation($"Shutting down server {dropletName}");
                    await _digitalOceanHelper.Shutdown(droplet.Id);
                    _logger.LogInformation($"Shut down server {dropletName}");
                }

                var snapshotName = $"{droplet.Name}_{Guid.NewGuid()}";
                _logger.LogInformation($"Snapshotting server {dropletName} as {snapshotName}");
                var snapshot = await _digitalOceanHelper.Snapshot(droplet.Id, snapshotName);
                if (snapshot == null) { throw new Exception($"Couldn't find snapshot name {snapshotName} just created"); }
                _logger.LogInformation($"Snapshotted server {dropletName} as {snapshot.Name} ({snapshot.Id})");

                if (server.SnapshotId.HasValue)
                {
                    _logger.LogInformation($"Deleting old snapshot id {server.SnapshotId}");
                    await _digitalOceanHelper.DeleteSnapshot(server.SnapshotId.Value);
                    _logger.LogInformation($"Deleted old snapshot id {server.SnapshotId}");
                }

                _logger.LogInformation($"Deleting server {dropletName}");
                await _digitalOceanHelper.DeleteDroplet(droplet.Id);
                _logger.LogInformation($"Deleted server {dropletName}");

                server.SnapshotId = snapshot.Id;
                server.Size = droplet.SizeSlug;
                server.Region = droplet.Region.Slug;
                server.DropletId = null;
                server.IPAddress = null;
                server.StatusId = ServerStatus.Inactive;
                await _serverRepository.Update(server);
                _logger.LogInformation($"Successfully stopped server {server.Id}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to stop server {server?.Id.ToString() ?? "N/A"}");
            }
            finally
            {
                if (server?.StatusId == ServerStatus.Deactivating)
                {
                    server.StatusId = ServerStatus.Active;
                    await _serverRepository.Update(server);
                }
            }
        }
    }
}
