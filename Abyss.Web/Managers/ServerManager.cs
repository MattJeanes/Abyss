using Abyss.Web.Data;
using Abyss.Web.Entities;
using Abyss.Web.Helpers.Interfaces;
using Abyss.Web.Logging;
using Abyss.Web.Managers.Interfaces;
using Abyss.Web.Repositories.Interfaces;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Abyss.Web.Managers;

public class ServerManager : IServerManager
{
    private readonly IDigitalOceanHelper _digitalOceanHelper;
    private readonly IRepository<Server> _serverRepository;
    private readonly ICloudflareHelper _cloudflareHelper;
    private readonly IAzureHelper _azureHelper;
    private readonly INotificationHelper _notificationHelper;
    private readonly ILogger<ServerManager> _baseLogger;

    public ServerManager(
        IDigitalOceanHelper digitalOceanHelper,
        IRepository<Server> serverRepository,
        IAzureHelper azureHelper,
        INotificationHelper notificationHelper,
        ICloudflareHelper cloudflareHelper,
        ILogger<ServerManager> logger
        )
    {
        _digitalOceanHelper = digitalOceanHelper;
        _serverRepository = serverRepository;
        _cloudflareHelper = cloudflareHelper;
        _azureHelper = azureHelper;
        _notificationHelper = notificationHelper;
        _baseLogger = logger;
    }

    public async Task<List<Server>> GetServers()
    {
        return await _serverRepository.GetAll().ToListAsync();
    }

    public async Task<Server> GetServerByAlias(string alias)
    {
        return await _serverRepository.GetAll()
            .Where(x => !string.IsNullOrEmpty(x.Alias) && x.Alias.ToLower() == alias.ToLower())
            .FirstOrDefaultAsync();
    }

    public async Task Start(string serverId, Func<LogItem, Task>? logHandler = null)
    {
        var logger = new TaskLogger(_baseLogger);
        if (logHandler != null)
        {
            logger.AddLogHandler("server-start", logHandler);
        }
        Server? server = null;
        try
        {
            server = await _serverRepository.GetById(serverId);
            if (server == null) { throw new Exception($"Server id {server} not found"); }
            if (string.IsNullOrEmpty(server.DNSRecord)) { throw new ArgumentNullException(nameof(server.DNSRecord)); }
            if (server.StatusId != ServerStatus.Inactive) { throw new Exception("Cannot start server that is active"); }
            logger.LogInformation($"Starting server {server.Id}");
            await _notificationHelper.SendMessage($"Starting server {server.Name}", MessagePriority.HighPriority);
            server.StatusId = ServerStatus.Activating;
            await _serverRepository.Update(server);

            string ipAddress;
            if (server.CloudType == CloudType.DigitalOcean)
            {
                if (!server.SnapshotId.HasValue) { throw new Exception("Server has no snapshot id"); }
                logger.LogInformation($"Creating droplet from server id {server.Id} - this may take a while... https://tenor.com/view/call-calling-dial-up-internet-modem-gif-8187684");
                var droplet = await _digitalOceanHelper.CreateDropletFromServer(server, logger);
                logger.LogInformation($"Created droplet from server id {server.Id}");
                server.DropletId = droplet.Id;
                ipAddress = droplet.Networks.V4.FirstOrDefault()?.IpAddress ?? throw new Exception("Droplet has no IPv4 address");
            }
            else if (server.CloudType == CloudType.Azure)
            {
                var vm = await _azureHelper.StartServer(server, logger);
                ipAddress = vm.GetPrimaryPublicIPAddress().IPAddress;
            }
            else
            {
                throw new Exception($"Unsupported cloud type {server.CloudType}");
            }

            var dnsRecord = await _cloudflareHelper.GetDNSRecord(server.DNSRecord);
            if (dnsRecord == null)
            {
                logger.LogWarning($"DNS record for {server.DNSRecord} does not exist and must be created");
            }
            else if (dnsRecord.Content != ipAddress)
            {
                logger.LogInformation($"Setting DNS record for {server.DNSRecord} to {ipAddress}");
                dnsRecord.Content = ipAddress;
                await _cloudflareHelper.UpdateDNSRecord(dnsRecord);
            }
            else
            {
                logger.LogInformation($"DNS record for {server.DNSRecord} already set to {ipAddress}");
            }

            server.StatusId = ServerStatus.Active;
            server.IPAddress = ipAddress;
            if (server.RemindAfterMinutes.HasValue)
            {
                server.NextReminder = DateTime.UtcNow.AddMinutes(server.RemindAfterMinutes.Value);
            }
            await _serverRepository.Update(server);
            logger.LogInformation($"Successfully started server {server.Id}");
            await _notificationHelper.SendMessage($"Started server {server.Name}", MessagePriority.HighPriority);
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Failed to start server {server?.Id.ToString() ?? "N/A"}");
            await _notificationHelper.SendMessage($"Failed to start server {server?.Name ?? "(Unknown)"}", MessagePriority.HighPriority);
            throw;
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

    public async Task Stop(string serverId, Func<LogItem, Task>? logHandler = null)
    {
        var logger = new TaskLogger(_baseLogger);
        if (logHandler != null)
        {
            logger.AddLogHandler("server-stop", logHandler);
        }
        Server? server = null;
        try
        {
            server = await _serverRepository.GetById(serverId);
            if (server == null) { throw new Exception($"Server id {server} not found"); }
            if (server.StatusId != ServerStatus.Active) { throw new Exception("Cannot stop server that is not active"); }
            logger.LogInformation($"Stopping server {server.Id}");
            await _notificationHelper.SendMessage($"Stopping server {server.Name}", MessagePriority.HighPriority);
            server.StatusId = ServerStatus.Deactivating;
            await _serverRepository.Update(server);
            if (server.CloudType == CloudType.DigitalOcean)
            {
                if (!server.DropletId.HasValue) { throw new Exception("Server has no droplet id"); }
                var droplet = await _digitalOceanHelper.GetDroplet(server.DropletId.Value);
                if (droplet == null) { throw new Exception($"Droplet id {server.DropletId} not valid"); }
                var dropletName = $"{droplet.Name} (id {droplet.Id})";
                if (!new[] { DigitalOceanEnums.DropletStatus.Active, DigitalOceanEnums.DropletStatus.Off }.Contains(droplet.Status)) { throw new Exception($"Server not in expected state, is in '{droplet.Status}'"); }
                if (droplet.Status == DigitalOceanEnums.DropletStatus.Active)
                {
                    logger.LogInformation($"Shutting down server {dropletName}");
                    await _digitalOceanHelper.Shutdown(droplet.Id);
                    logger.LogInformation($"Shut down server {dropletName}");
                }

                var snapshotName = $"{droplet.Name}_{Guid.NewGuid()}";
                logger.LogInformation($"Snapshotting server {dropletName} as {snapshotName} - this may take a while... https://tenor.com/view/call-calling-dial-up-internet-modem-gif-8187684");
                var snapshot = await _digitalOceanHelper.Snapshot(droplet.Id, snapshotName);
                if (snapshot == null) { throw new Exception($"Couldn't find snapshot name {snapshotName} just created"); }
                logger.LogInformation($"Snapshotted server {dropletName} as {snapshot.Name} ({snapshot.Id})");

                if (server.SnapshotId.HasValue)
                {
                    logger.LogInformation($"Deleting old snapshot id {server.SnapshotId}");
                    await _digitalOceanHelper.DeleteSnapshot(server.SnapshotId.Value);
                }

                logger.LogInformation($"Deleting server {dropletName}");
                await _digitalOceanHelper.DeleteDroplet(droplet.Id);

                server.SnapshotId = snapshot.Id;
                if (!string.IsNullOrEmpty(server.Resize))
                {
                    server.Resize = droplet.SizeSlug;
                }
                else
                {
                    server.Size = droplet.SizeSlug;
                }
                server.Region = droplet.Region.Slug;
                server.DropletId = null;
            }

            else if (server.CloudType == CloudType.Azure)
            {
                await _azureHelper.StopServer(server, logger);
            }
            else
            {
                throw new Exception($"Unsupported cloud type {server.CloudType}");
            }
            server.IPAddress = null;
            server.NextReminder = null;
            server.StatusId = ServerStatus.Inactive;
            await _serverRepository.Update(server);
            logger.LogInformation($"Successfully stopped server {server.Id}");
            await _notificationHelper.SendMessage($"Stopped server {server.Name}", MessagePriority.HighPriority);
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Failed to stop server {server?.Id.ToString() ?? "N/A"}");
            await _notificationHelper.SendMessage($"Failed to stop server {server?.Name ?? "(Unknown)"}", MessagePriority.HighPriority);
            throw;
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
