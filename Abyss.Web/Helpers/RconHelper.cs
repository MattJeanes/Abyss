using Abyss.Web.Data;
using Abyss.Web.Entities;
using Abyss.Web.Helpers.Interfaces;
using CoreRCON;
using System.Net;
using System.Net.Sockets;

namespace Abyss.Web.Helpers;

public class RconHelper(ILogger<RconHelper> logger, INotificationHelper notificationHelper) : IRconHelper
{
    private readonly ILogger<RconHelper> _logger = logger;
    private readonly INotificationHelper _notificationHelper = notificationHelper;

    public async Task<string> ExecuteCommand(Server server, string command)
    {
        if (!SupportsServerType(server.Type))
        {
            throw new Exception($"RCON is not supported for server type: {server.Type}");
        }

        if (string.IsNullOrEmpty(server.ApiBaseUrl))
        {
            throw new Exception("Server RCON host is not configured");
        }

        if (string.IsNullOrEmpty(server.ApiKey))
        {
            throw new Exception("Server RCON password is not configured");
        }

        var host = server.ApiBaseUrl;

        if (!host.Contains(':'))
        {
            throw new Exception("Server does not contain port number");
        }

        var parts = host.Split(':');
        host = parts[0];
        if (!int.TryParse(parts[1], out var port))
        {
            throw new Exception($"Invalid server port: ${parts[1]}");
        }

        try
        {
            _logger.LogInformation($"Executing RCON command on server {server.Name} ({host}:{port}): {command}");

            if (!IPAddress.TryParse(host, out var ipAddress))
            {

                var ipHostEntry = Dns.GetHostEntry(host);
                ipAddress = ipHostEntry.AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
                if (ipAddress == null)
                {
                    throw new Exception($"Unable to resolve host: {host}");
                }
            }
            var endpoint = new IPEndPoint(ipAddress, port);
            using var rcon = new RCON(endpoint, server.ApiKey);
            await rcon.ConnectAsync();

            var response = await rcon.SendCommandAsync(command);
            _logger.LogInformation($"RCON command response: {response}");

            var formattedResponse = string.IsNullOrEmpty(response) ? "Command executed successfully (no response)" : response;
            
            await _notificationHelper.SendMessage($"RCON Command on {server.Name}\nCommand: {command}\nResponse: {formattedResponse}", MessagePriority.Normal);

            return formattedResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error executing RCON command on server {server.Name}: {command}");
            
            await _notificationHelper.SendMessage($"RCON Command Error on {server.Name}\nCommand: {command}\nError: {ex.Message}", MessagePriority.HighPriority);
            
            return $"Error executing command: {ex.Message}";
        }
    }

    public bool SupportsServerType(ServerType serverType)
    {
        return serverType switch
        {
            ServerType.GMod => true,
            ServerType.CSGO => true,
            ServerType.Minecraft => true,
            _ => false,
        };
    }
}