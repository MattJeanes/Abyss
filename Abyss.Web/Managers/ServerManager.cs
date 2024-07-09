using Abyss.Web.Data;
using Abyss.Web.Entities;
using Abyss.Web.Helpers.Interfaces;
using Abyss.Web.Logging;
using Abyss.Web.Managers.Interfaces;
using Abyss.Web.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Abyss.Web.Managers;

public class ServerManager(
    IRepository<Server> serverRepository,
    IAzureHelper azureHelper,
    INotificationHelper notificationHelper,
    ICloudflareHelper cloudflareHelper,
    ILogger<ServerManager> logger,
    ISpaceEngineersHelper spaceEngineersHelper,
    IMinecraftHelper minecraftHelper
        ) : IServerManager
{
    private readonly IRepository<Server> _serverRepository = serverRepository;
    private readonly ICloudflareHelper _cloudflareHelper = cloudflareHelper;
    private readonly IAzureHelper _azureHelper = azureHelper;
    private readonly INotificationHelper _notificationHelper = notificationHelper;
    private readonly ILogger<ServerManager> _baseLogger = logger;
    private readonly ISpaceEngineersHelper _spaceEngineersHelper = spaceEngineersHelper;
    private readonly IMinecraftHelper _minecraftHelper = minecraftHelper;

    public async Task<List<Server>> GetServers()
    {
        return await _serverRepository.GetAll().OrderBy(x => x.Id).ToListAsync();
    }

    public async Task<Server> GetServerByAlias(string alias)
    {
        return await _serverRepository.GetAll()
            .Where(x => !string.IsNullOrEmpty(x.Alias) && x.Alias.ToLower() == alias.ToLower())
            .FirstOrDefaultAsync();
    }

    public async Task Start(int serverId, Func<LogItem, Task> logHandler = null, Func<ServerStatus, Task> statusHandler = null)
    {
        var logger = new TaskLogger(_baseLogger);
        if (logHandler != null)
        {
            logger.AddLogHandler("server-start", logHandler);
        }
        Server server = null;
        try
        {
            server = await _serverRepository.GetById(serverId);
            if (server == null) { throw new Exception($"Server id {server} not found"); }
            if (string.IsNullOrEmpty(server.DNSRecord)) { throw new ArgumentNullException(nameof(server.DNSRecord)); }
            if (server.StatusId != ServerStatus.Inactive) { throw new Exception("Cannot start server that is active"); }
            logger.LogInformation($"Starting server {server.Name}");
            await _notificationHelper.SendMessage($"Starting server {server.Name}", MessagePriority.HighPriority);
            server.StatusId = ServerStatus.Activating;
            await _serverRepository.SaveChanges();
            if (statusHandler != null)
            {
                await statusHandler(server.StatusId);
            }

            string ipAddress;
            if (server.CloudType == CloudType.Azure)
            {
                await _azureHelper.StartServer(server, logger);
                ipAddress = await _azureHelper.GetServerIpAddress(server);
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
            await _serverRepository.SaveChanges();
            if (statusHandler != null)
            {
                await statusHandler(server.StatusId);
            }
            logger.LogInformation($"Successfully started server {server.Name}");
            await _notificationHelper.SendMessage($"Started server {server.Name}", MessagePriority.HighPriority);
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Failed to start server {server?.Name ?? "N/A"}");
            await _notificationHelper.SendMessage($"Failed to start server {server?.Name ?? "(Unknown)"}", MessagePriority.HighPriority);
            throw;
        }
        finally
        {
            if (server?.StatusId == ServerStatus.Activating)
            {
                server.StatusId = ServerStatus.Inactive;
                await _serverRepository.SaveChanges();
                if (statusHandler != null)
                {
                    await statusHandler(server.StatusId);
                }
            }
        }

    }

    public async Task Stop(int serverId, Func<LogItem, Task> logHandler = null, Func<ServerStatus, Task> statusHandler = null)
    {
        var logger = new TaskLogger(_baseLogger);
        if (logHandler != null)
        {
            logger.AddLogHandler("server-stop", logHandler);
        }
        Server server = null;
        try
        {
            server = await _serverRepository.GetById(serverId);
            if (server == null) { throw new Exception($"Server id {server} not found"); }
            if (server.StatusId != ServerStatus.Active) { throw new Exception("Cannot stop server that is not active"); }
            logger.LogInformation($"Stopping server {server.Name}");
            await _notificationHelper.SendMessage($"Stopping server {server.Name}", MessagePriority.HighPriority);
            server.StatusId = ServerStatus.Deactivating;
            await _serverRepository.SaveChanges();
            if (statusHandler != null)
            {
                await statusHandler(server.StatusId);
            }
            if (server.CloudType == CloudType.Azure)
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
            await _serverRepository.SaveChanges();
            if (statusHandler != null)
            {
                await statusHandler(server.StatusId);
            }
            logger.LogInformation($"Successfully stopped server {server.Name}");
            await _notificationHelper.SendMessage($"Stopped server {server.Name}", MessagePriority.HighPriority);
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Failed to stop server {server?.Name ?? "N/A"}");
            await _notificationHelper.SendMessage($"Failed to stop server {server?.Name ?? "(Unknown)"}", MessagePriority.HighPriority);
            throw;
        }
        finally
        {
            if (server?.StatusId == ServerStatus.Deactivating)
            {
                server.StatusId = ServerStatus.Active;
                await _serverRepository.SaveChanges();
                if (statusHandler != null)
                {
                    await statusHandler(server.StatusId);
                }
            }
        }
    }

    public async Task Restart(int serverId, Func<LogItem, Task> logHandler = null)
    {
        var logger = new TaskLogger(_baseLogger);
        if (logHandler != null)
        {
            logger.AddLogHandler("server-restart", logHandler);
        }
        Server server = null;
        try
        {
            server = await _serverRepository.GetById(serverId);
            if (server == null) { throw new Exception($"Server id {server} not found"); }
            if (server.StatusId != ServerStatus.Active) { throw new Exception("Cannot restart server that is not active"); }
            logger.LogInformation($"Restarting server {server.Name}");
            await _notificationHelper.SendMessage($"Restarting server {server.Name}", MessagePriority.HighPriority);
            if (server.CloudType == CloudType.Azure)
            {
                await _azureHelper.RestartServer(server, logger);
            }
            else
            {
                throw new Exception($"Unsupported cloud type {server.CloudType}");
            }
            logger.LogInformation($"Successfully restarted server {server.Name}");
            await _notificationHelper.SendMessage($"Restarted server {server.Name}", MessagePriority.HighPriority);
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Failed to restart server {server?.Name ?? "N/A"}");
            await _notificationHelper.SendMessage($"Failed to restart server {server?.Name ?? "(Unknown)"}", MessagePriority.HighPriority);
            throw;
        }
    }

    public async Task<ServerRichStatus> GetServerRichStatus(Server server)
    {
        if (server.StatusId != ServerStatus.Active) { return null; }
        try
        {
            if (server.Type == ServerType.SpaceEngineers)
            {
                var characters = await _spaceEngineersHelper.GetCharacters(server);
                return new ServerRichStatus
                {
                    Players = characters.Select(x => x.DisplayName).ToList(),
                };
            }
            else if (server.Type == ServerType.Minecraft)
            {
                var players = await _minecraftHelper.GetPlayers(server.DNSRecord);
                return new ServerRichStatus
                {
                    Players = players
                };
            }
        }
        catch (Exception e)
        {
            _baseLogger.LogError(e, $"Failed to get rich status for server: {server.Name}");
            return new ServerRichStatus
            {
                Error = e.Message
            };
        }
        return null;
    }
}
