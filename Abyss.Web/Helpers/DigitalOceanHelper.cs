﻿using Abyss.Web.Data;
using Abyss.Web.Data.Options;
using Abyss.Web.Entities;
using Abyss.Web.Helpers.Interfaces;
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

namespace Abyss.Web.Helpers
{
    public class DigitalOceanHelper : IDigitalOceanHelper
    {

        private readonly DigitalOceanClient _client;
        private readonly DigitalOceanOptions _options;
        private readonly ILogger<DigitalOceanHelper> _logger;

        public DigitalOceanHelper(DigitalOceanClient client, IOptions<DigitalOceanOptions> options, ILogger<DigitalOceanHelper> logger)
        {
            _client = client;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<Droplet> CreateDropletFromServer(Server server)
        {
            if (!server.SnapshotId.HasValue) { throw new ArgumentNullException(nameof(server.SnapshotId)); }
            var servers = await _client.Droplets.GetAllByTag(server.Tag);
            if (servers.Any()) { throw new Exception($"Droplet {server.Tag} already exists"); }
            var newDroplet = new Requests.Droplet
            {
                ImageIdOrSlug = server.SnapshotId.Value,
                Name = server.Tag,
                SizeSlug = server.Size,
                RegionSlug = server.Region,
                Backups = true,
                Tags = new List<object> { server.Tag },
                SshIdsOrFingerprints = new List<object> { _options.SshId }
            };
            var droplet = await WaitForDropletCreation(await _client.Droplets.Create(newDroplet));
            if (!string.IsNullOrEmpty(server.Resize))
            {
                _logger.LogInformation($"Shutting down droplet id {droplet.Id} for resize");
                await WaitForAction(await _client.DropletActions.Shutdown(droplet.Id));
                _logger.LogInformation($"Resizing droplet id {droplet.Id} to {server.Resize}");
                await WaitForAction(await _client.DropletActions.Resize(droplet.Id, server.Resize));
                _logger.LogInformation($"Powering on droplet id {droplet.Id}");
                await WaitForAction(await _client.DropletActions.PowerOn(droplet.Id));
            }
            return droplet;
        }

        public async Task<Droplet> GetDroplet(int id)
        {
            return await _client.Droplets.Get(id);
        }

        public async Task Shutdown(int dropletId)
        {
            await WaitForAction(await _client.DropletActions.Shutdown(dropletId));
        }

        public async Task<Image> Snapshot(int dropletId, string snapshotName)
        {
            await WaitForAction(await _client.DropletActions.Snapshot(dropletId, snapshotName));
            var images = await _client.Images.GetAll(Requests.ImageType.Private);
            var snapshot = images.FirstOrDefault(x => x.Type == DigitalOceanEnums.ImageType.Snapshot && x.Name == snapshotName);
            return snapshot;
        }

        public async Task DeleteSnapshot(int id)
        {
            await _client.Images.Delete(id);
        }

        public async Task DeleteDroplet(int id)
        {
            await _client.Droplets.Delete(id);
        }

        private async Task<Droplet> WaitForDropletCreation(Droplet droplet)
        {
            await Wait(() => droplet.Status == DigitalOceanEnums.DropletStatus.New, async () => { droplet = await _client.Droplets.Get(droplet.Id); });
            return droplet;
        }

        private async Task<Responses.Action> WaitForAction(Responses.Action action)
        {
            await Wait(() => action.Status == DigitalOceanEnums.ActionStatus.InProgress, async () => { action = await _client.Actions.Get(action.Id); });
            if (action.Status == DigitalOceanEnums.ActionStatus.Errored)
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
            if (timerElapsed)
            {
                throw new TimeoutException($"Wait exceeded timeout of {timeout.Value}s");
            }
        }
    }
}