using Abyss.Web.Data.Options;
using Abyss.Web.Entities;
using Abyss.Web.Helpers.Interfaces;
using Abyss.Web.Logging;
using Azure;
using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.Compute;
using Azure.ResourceManager.Network;
using Microsoft.Extensions.Options;

namespace Abyss.Web.Helpers;

public class AzureHelper(ArmClient azure, IOptions<AzureOptions> options) : IAzureHelper
{
    private readonly ArmClient _azure = azure;
    private readonly AzureOptions _options = options.Value;

    public async Task<VirtualMachineResource> StartServer(Server server, TaskLogger logger)
    {
        var vm = _azure.GetVirtualMachineResource(new ResourceIdentifier(server.ResourceId));
        logger.LogInformation($"Starting Azure virtual machine {vm.Id.Name} in resource group {vm.Id.ResourceGroupName}");
        await vm.PowerOnAsync(WaitUntil.Completed, GetCancellationToken());
        logger.LogInformation($"Virtual machine {vm.Id.Name} started");
        return vm;
    }

    public async Task<VirtualMachineResource> StopServer(Server server, TaskLogger logger)
    {
        var vm = _azure.GetVirtualMachineResource(new ResourceIdentifier(server.ResourceId));
        logger.LogInformation($"Shutting down Azure virtual machine {vm.Id.Name} in resource group {vm.Id.ResourceGroupName}");
        await vm.DeallocateAsync(WaitUntil.Completed, false, GetCancellationToken());
        logger.LogInformation($"Virtual machine {vm.Id.Name} stopped");
        return vm;
    }

    public async Task<VirtualMachineResource> RestartServer(Server server, TaskLogger logger)
    {
        var vm = _azure.GetVirtualMachineResource(new ResourceIdentifier(server.ResourceId));
        logger.LogInformation($"Restarting Azure virtual machine {vm.Id.Name} in resource group {vm.Id.ResourceGroupName}");
        await vm.RestartAsync(WaitUntil.Completed, GetCancellationToken());
        logger.LogInformation($"Virtual machine {vm.Id.Name} restarted");
        return vm;
    }

    public async Task<string> GetServerIpAddress(Server server)
    {
        var vm = (await _azure.GetVirtualMachineResource(new ResourceIdentifier(server.ResourceId)).GetAsync()).Value;
        var networkInterface = (await _azure.GetNetworkInterfaceResource(vm.Data.NetworkProfile.NetworkInterfaces.First().Id).GetAsync()).Value;
        var networkInterfaceIPConfiguration = (await _azure.GetNetworkInterfaceIPConfigurationResource(networkInterface.GetNetworkInterfaceIPConfigurations().First().Id).GetAsync()).Value;
        var ipAddress = (await _azure.GetPublicIPAddressResource(networkInterfaceIPConfiguration.Data.PublicIPAddress.Id).GetAsync()).Value;
        return ipAddress.Data.IPAddress;
    }

    private CancellationToken GetCancellationToken()
    {
        return new CancellationTokenSource(TimeSpan.FromSeconds(_options.OperationTimeoutSeconds)).Token;
    }
}
