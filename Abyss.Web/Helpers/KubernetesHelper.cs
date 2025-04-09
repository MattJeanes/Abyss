using Abyss.Web.Data.Options;
using Abyss.Web.Entities;
using Abyss.Web.Helpers.Interfaces;
using Abyss.Web.Logging;
using k8s;
using Microsoft.Extensions.Options;

namespace Abyss.Web.Helpers;

public class KubernetesHelper : IKubernetesHelper
{
    private readonly Kubernetes _client;
    private readonly ILogger<KubernetesHelper> _logger;
    private readonly KubernetesOptions _options;
    private readonly HttpClient _httpClient;

    public KubernetesHelper(IOptions<KubernetesOptions> options, ILogger<KubernetesHelper> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _options = options.Value;
        _httpClient = httpClientFactory.CreateClient("ipservice");

        KubernetesClientConfiguration config;

        if (!string.IsNullOrEmpty(_options.ApiUrl) && !string.IsNullOrEmpty(_options.ApiToken))
        {
            config = new KubernetesClientConfiguration
            {
                Host = _options.ApiUrl,
                AccessToken = _options.ApiToken,
                SkipTlsVerify = true
            };

            _logger.LogInformation("Using external Kubernetes configuration with provided API URL and token");
        }
        else
        {
            config = KubernetesClientConfiguration.InClusterConfig();
            _logger.LogInformation("Using in-cluster Kubernetes configuration");
        }

        _client = new Kubernetes(config);
    }

    public async Task StartServer(Server server, TaskLogger logger)
    {
        logger.LogInformation("Starting Kubernetes resource...");

        try
        {
            var (namespace_, kind, name) = ParseResourceId(server.ResourceId);

            switch (kind.ToLower())
            {
                case "deployment":
                case "deployments":
                    var deployment = await _client.ReadNamespacedDeploymentAsync(name, namespace_);
                    var replicas = deployment.Spec.Replicas.GetValueOrDefault(0);

                    if (replicas > 0)
                    {
                        logger.LogInformation($"Deployment {name} already has {replicas} replicas");
                        return;
                    }

                    deployment.Spec.Replicas = 1;
                    await _client.ReplaceNamespacedDeploymentAsync(deployment, name, namespace_);
                    logger.LogInformation($"Scaled deployment {name} to 1 replica");
                    break;

                case "statefulset":
                case "statefulsets":
                    var statefulSet = await _client.ReadNamespacedStatefulSetAsync(name, namespace_);
                    var ssReplicas = statefulSet.Spec.Replicas.GetValueOrDefault(0);

                    if (ssReplicas > 0)
                    {
                        logger.LogInformation($"StatefulSet {name} already has {ssReplicas} replicas");
                        return;
                    }

                    statefulSet.Spec.Replicas = 1;
                    await _client.ReplaceNamespacedStatefulSetAsync(statefulSet, name, namespace_);
                    logger.LogInformation($"Scaled statefulset {name} to 1 replica");
                    break;

                default:
                    throw new NotSupportedException($"Resource kind '{kind}' is not supported for starting");
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Error starting Kubernetes resource: {ex.Message}");
            _logger.LogError(ex, "Error starting Kubernetes resource {ResourceId}", server.ResourceId);
            throw;
        }
    }

    public async Task StopServer(Server server, TaskLogger logger)
    {
        logger.LogInformation("Stopping Kubernetes resource...");

        try
        {
            var (namespace_, kind, name) = ParseResourceId(server.ResourceId);

            switch (kind.ToLower())
            {
                case "deployment":
                case "deployments":
                    var deployment = await _client.ReadNamespacedDeploymentAsync(name, namespace_);

                    deployment.Spec.Replicas = 0;
                    await _client.ReplaceNamespacedDeploymentAsync(deployment, name, namespace_);
                    logger.LogInformation($"Scaled deployment {name} to 0 replicas");
                    break;

                case "statefulset":
                case "statefulsets":
                    var statefulSet = await _client.ReadNamespacedStatefulSetAsync(name, namespace_);

                    statefulSet.Spec.Replicas = 0;
                    await _client.ReplaceNamespacedStatefulSetAsync(statefulSet, name, namespace_);
                    logger.LogInformation($"Scaled statefulset {name} to 0 replicas");
                    break;

                default:
                    throw new NotSupportedException($"Resource kind '{kind}' is not supported for stopping");
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Error stopping Kubernetes resource: {ex.Message}");
            _logger.LogError(ex, "Error stopping Kubernetes resource {ResourceId}", server.ResourceId);
            throw;
        }
    }

    public async Task RestartServer(Server server, TaskLogger logger)
    {
        logger.LogInformation("Restarting Kubernetes resource...");

        try
        {
            var (namespace_, kind, name) = ParseResourceId(server.ResourceId);

            switch (kind.ToLower())
            {
                case "deployment":
                case "deployments":
                    var deployment = await _client.ReadNamespacedDeploymentAsync(name, namespace_);

                    if (deployment.Spec.Template.Metadata.Annotations == null)
                    {
                        deployment.Spec.Template.Metadata.Annotations = new Dictionary<string, string>();
                    }

                    deployment.Spec.Template.Metadata.Annotations["kubectl.kubernetes.io/restartedAt"] = DateTime.UtcNow.ToString("o");
                    await _client.ReplaceNamespacedDeploymentAsync(deployment, name, namespace_);
                    logger.LogInformation($"Triggered restart of deployment {name}");
                    break;

                case "statefulset":
                case "statefulsets":
                    var statefulSet = await _client.ReadNamespacedStatefulSetAsync(name, namespace_);

                    if (statefulSet.Spec.Template.Metadata.Annotations == null)
                    {
                        statefulSet.Spec.Template.Metadata.Annotations = new Dictionary<string, string>();
                    }

                    statefulSet.Spec.Template.Metadata.Annotations["kubectl.kubernetes.io/restartedAt"] = DateTime.UtcNow.ToString("o");
                    await _client.ReplaceNamespacedStatefulSetAsync(statefulSet, name, namespace_);
                    logger.LogInformation($"Triggered restart of statefulset {name}");
                    break;

                default:
                    throw new NotSupportedException($"Resource kind '{kind}' is not supported for restarting");
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Error restarting Kubernetes resource: {ex.Message}");
            _logger.LogError(ex, "Error restarting Kubernetes resource {ResourceId}", server.ResourceId);
            throw;
        }
    }

    public async Task<string> GetServerIpAddress(Server server)
    {
        try
        {
            var response = await _httpClient.GetStringAsync("https://api.ipify.org");
            var publicIp = response.Trim();

            if (!string.IsNullOrEmpty(publicIp))
            {
                return publicIp;
            }

            return "No IP available";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting public IP address for {ResourceId}", server.ResourceId);
            return "Error getting IP address";
        }
    }

    private (string Namespace, string Kind, string Name) ParseResourceId(string resourceId)
    {
        if (string.IsNullOrEmpty(resourceId))
        {
            throw new ArgumentException("ResourceId cannot be null or empty", nameof(resourceId));
        }

        if (resourceId.StartsWith("/"))
        {
            resourceId = resourceId.Substring(1);
        }

        var parts = resourceId.Split('/');

        if (parts.Length != 4 || parts[0] != "namespaces")
        {
            throw new ArgumentException("Invalid ResourceId format. Expected format: /namespaces/{namespace}/{kind}/{name}", nameof(resourceId));
        }

        var namespace_ = parts[1];
        var kind = parts[2];
        var name = parts[3];

        return (namespace_, kind, name);
    }
}