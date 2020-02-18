using Abyss.Web.Data.Options;
using Abyss.Web.Entities;
using Abyss.Web.Helpers.Interfaces;
using Abyss.Web.Logging;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Abyss.Web.Helpers
{
    public class AzureHelper : IAzureHelper
    {
        private readonly IAzure _azure;
        private readonly AzureOptions _options;

        public AzureHelper(IAzure azure, IOptions<AzureOptions> options)
        {
            _azure = azure;
            _options = options.Value;
        }

        public async Task<IVirtualMachine> StartServer(Server server, TaskLogger logger)
        {
            var vm = await _azure.VirtualMachines.GetByIdAsync(server.ResourceId);
            logger.LogInformation($"Starting Azure virtual machine {vm.Name} in resource group {vm.ResourceGroupName}");
            await vm.StartAsync(GetCancellationToken());
            logger.LogInformation($"Virtual machine started");
            return vm;
        }

        public async Task<IVirtualMachine> StopServer(Server server, TaskLogger logger)
        {
            var vm = await _azure.VirtualMachines.GetByIdAsync(server.ResourceId);
            logger.LogInformation($"Shutting down Azure virtual machine {vm.Name} in resource group {vm.ResourceGroupName}");
            await vm.DeallocateAsync(GetCancellationToken());
            logger.LogInformation($"Virtual machine stopped");
            return vm;
        }

        private CancellationToken GetCancellationToken()
        {
            return new CancellationTokenSource(TimeSpan.FromSeconds(_options.OperationTimeoutSeconds)).Token;
        }
    }
}
