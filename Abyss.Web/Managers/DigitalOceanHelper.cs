using Abyss.Web.Data;
using Abyss.Web.Data.Options;
using Abyss.Web.Entities;
using Abyss.Web.Managers.Interfaces;
using Abyss.Web.Repositories.Interfaces;
using DigitalOcean.API;
using DigitalOcean.API.Models.Responses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Requests = DigitalOcean.API.Models.Requests;
using Responses = DigitalOcean.API.Models.Responses;

namespace Abyss.Web.Managers
{
    public class DigitalOceanHelper : IDigitalOceanHelper
    {

        private readonly DigitalOceanClient _client;
        private readonly DigitalOceanOptions _options;
        private readonly ILogger _logger;
        private readonly IRepository<Server> _serverRepository;

        public DigitalOceanHelper(DigitalOceanClient client, IOptions<DigitalOceanOptions> options, ILogger<DigitalOceanHelper> logger, IRepository<Server> serverRepository)
        {
            _client = client;
            _options = options.Value;
            _logger = logger;
            _serverRepository = serverRepository;
        }

        public async Task CreateDropletFromServerAndDeleteSnapshot(Server server)
        {
            try
            {
                if (server.StatusId != ServerStatus.Inactive) { throw new Exception("Cannot create server that is active"); }
                if (!server.SnapshotId.HasValue) { throw new Exception("Server has no snapshot id"); }
                var image = await _client.Images.Get(server.SnapshotId.Value);
                if (image == null) { throw new Exception($"Snapshot id {server.SnapshotId} not valid"); }
                _logger.LogDebug($"Found image {image.Name} ({image.Id})");
                server.StatusId = ServerStatus.Activating;
                await _serverRepository.Update(server);
                var newDroplet = new Requests.Droplet
                {
                    ImageIdOrSlug = image.Id,
                    Name = server.Tag,
                    SizeSlug = server.Size,
                    RegionSlug = server.Region,
                    Backups = true,
                    Tags = new List<object> { server.Tag },
                    SshIdsOrFingerprints = new List<object> { _options.SshId }
                };
                _logger.LogInformation($"Creating droplet {newDroplet.Name} from server id {server.Id}");
                var droplet = await WaitForDropletCreation(await _client.Droplets.Create(newDroplet));
                _logger.LogInformation($"Created droplet {newDroplet.Name} from server id {server.Id}");

                server.DropletId = droplet.Id;
                server.StatusId = ServerStatus.Active;
                server.IPAddress = droplet.Networks.v4.FirstOrDefault()?.IpAddress ?? throw new Exception("Droplet has no IPv4 address");
                await _serverRepository.Update(server);
            }
            finally
            {
                if (server.StatusId == ServerStatus.Activating)
                {
                    server.StatusId = ServerStatus.Inactive;
                    await _serverRepository.Update(server);
                }
            }
        }

        public async Task DeleteAndSnapshotDroplet(Server server)
        {
            try
            {
                if (server.StatusId != ServerStatus.Active) { throw new Exception("Cannot delete server that is not active"); }
                if (!server.DropletId.HasValue) { throw new Exception("Server has no droplet id"); }
                var droplet = await _client.Droplets.Get(server.DropletId.Value);
                if (droplet == null) { throw new Exception($"Droplet id {server.DropletId} not valid"); }
                var dropletName = $"{droplet.Name} (id {droplet.Id})";
                _logger.LogDebug($"Found droplet {dropletName}");
                if (!new[] { DropletStatus.Active, DropletStatus.Off }.Contains(droplet.Status)) { throw new Exception($"Server not in expected state, is in '{droplet.Status}'"); }
                server.StatusId = ServerStatus.Deactivating;
                await _serverRepository.Update(server);

                if (droplet.Status == DropletStatus.Active)
                {
                    _logger.LogInformation($"Powering off server {dropletName}");
                    await WaitForAction(await _client.DropletActions.Shutdown(droplet.Id));
                    _logger.LogInformation($"Powered off server {dropletName}");
                }

                var snapshotName = $"{droplet.Name}_{Guid.NewGuid()}";
                _logger.LogInformation($"Snapshotting server {dropletName} as {snapshotName}");
                await WaitForAction(await _client.DropletActions.Snapshot(droplet.Id, snapshotName));
                var images = await _client.Images.GetAll(Requests.ImageType.Private);
                var snapshot = images.FirstOrDefault(x => x.Type == ImageType.Snapshot && x.Name == snapshotName);
                if (snapshot == null) { throw new Exception($"Couldn't find snapshot name {snapshotName} just created"); }
                _logger.LogInformation($"Snapshotted server {dropletName} as {snapshot.Name} ({snapshot.Id})");

                if (server.SnapshotId.HasValue)
                {
                    _logger.LogInformation($"Deleting old snapshot id {server.SnapshotId}");
                    await _client.Images.Delete(server.SnapshotId.Value);
                    _logger.LogInformation($"Deleted old snapshot id {server.SnapshotId}");
                }

                _logger.LogInformation($"Deleting server {dropletName}");
                await _client.Droplets.Delete(droplet.Id);
                _logger.LogInformation($"Deleted server {dropletName}");

                server.SnapshotId = snapshot.Id;
                server.Size = droplet.SizeSlug;
                server.Region = droplet.Region.Slug;
                server.DropletId = null;
                server.IPAddress = null;
                server.StatusId = ServerStatus.Inactive;
                await _serverRepository.Update(server);
            }
            finally
            {
                if (server.StatusId == ServerStatus.Deactivating)
                {
                    server.StatusId = ServerStatus.Active;
                    await _serverRepository.Update(server);
                }
            }
        }

        private async Task<Droplet> WaitForDropletCreation(Droplet droplet)
        {
            await Wait(() => droplet.Status == DropletStatus.New, async () => { droplet = await _client.Droplets.Get(droplet.Id); });
            return droplet;
        }

        private async Task<Responses.Action> WaitForAction(Responses.Action action)
        {
            await Wait(() => action.Status == ActionStatus.InProgress, async () => { action = await _client.Actions.Get(action.Id); });
            if (action.Status == ActionStatus.Errored)
            {
                throw new Exception($"Action {action.Id} ({action.Type}) on {action.ResourceId} ({action.ResourceType}) failed, was started at {action.StartedAt}");
            }
            return action;
        }

        private async Task Wait(Func<bool> shouldLoop, Func<Task> check, int? timeout = null, int? timeBetweenChecks = null)
        {
            if (!timeout.HasValue)
            {
                timeout = _options.ActionTimeout;
            }
            if (!timeBetweenChecks.HasValue)
            {
                timeBetweenChecks = _options.TimeBetweenChecks;
            }
            var timer = new Timer(timeout.Value * 1000)
            {
                AutoReset = false,
            };
            var timerElapsed = false;
            timer.Elapsed += (sender, e) => { timerElapsed = true; };
            timer.Start();
            while (shouldLoop() && !timerElapsed)
            {
                await Task.Delay(timeBetweenChecks.Value * 1000);
                await check();
            }
        }

        private class ActionStatus
        {
            public const string InProgress = "in-progress";
            public const string Completed = "completed";
            public const string Errored = "errored";
        }

        private class DropletStatus
        {
            public const string New = "new";
            public const string Active = "active";
            public const string Off = "off";
            public const string Archive = "archive";
        }

        private class ImageType
        {
            public const string Snapshot = "snapshot";
            public const string Backup = "backup";
        }
    }
}
