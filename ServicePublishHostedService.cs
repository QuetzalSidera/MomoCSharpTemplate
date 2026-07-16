using Shared.Contracts.Capabilities;
using Shared.Contracts.Policies;
using Shared.GatewaySdk.ServicePublishing;

internal sealed class ServicePublishHostedService(
    IConfiguration configuration,
    GatewayServicePublishClient publishClient,
    ServiceCommandRegistry<ServiceCommandExecutor> commandRegistry,
    ILogger<ServicePublishHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!configuration.GetValue<bool>("Gateway:PublishOnStartup"))
            return;

        var version = ReadServiceVersion(configuration);
        await publishClient.PublishAsync(new PublishGatewayServiceRequest
        {
            ServiceName = ServiceTemplateConfiguration.ReadRequiredString(configuration, "Service:Name"),
            DeploymentId = ServiceTemplateConfiguration.ReadRequiredString(configuration, "Service:DeploymentId"),
            DisplayName = ServiceTemplateConfiguration.ReadRequiredString(configuration, "Service:DisplayName"),
            Description = ServiceTemplateConfiguration.ReadRequiredString(configuration, "Service:Description"),
            Version = version.ToString(),
            ConsumedEvents =
                ServiceTemplateConfiguration.ReadConsumedEventCapabilityList(configuration,
                    "Service:ConsumedEvents"),
            RequiredActions =
                ServiceTemplateConfiguration.ReadActionCapabilityList(configuration,
                    "Service:RequiredActions"),
            PreferredActions =
                ServiceTemplateConfiguration.ReadActionCapabilityList(configuration,
                    "Service:PreferredActions"),
            OfferedPolicies =
                ServiceTemplateConfiguration.ReadGatewayPolicyTypeList(configuration,
                    "Service:OfferedPolicies"),
            RequiredBotPolicies =
                ServiceTemplateConfiguration.ReadGatewayPolicyTypeList(configuration,
                    "Service:RequiredBotPolicies"),
            PreferredBotPolicies =
                ServiceTemplateConfiguration.ReadGatewayPolicyTypeList(configuration,
                    "Service:PreferredBotPolicies"),
            RouteHints = commandRegistry.ToRouteHints(),
            ServiceConfigMetadata = ServiceCommandMetadata.FromRegistry(commandRegistry)
        }, cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (!configuration.GetValue<bool>("Gateway:ReportOfflineOnShutdown"))
            return;

        try
        {
            await publishClient.OfflineAsync(new OfflineGatewayServiceRequest
            {
                DeploymentId = configuration.GetValue<string>("Service:DeploymentId")
            }, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Failed to report service offline to Gateway.");
        }
    }

    private static ServiceVersion ReadServiceVersion(IConfiguration configuration)
    {
        var value = ServiceTemplateConfiguration.ReadRequiredString(configuration, "Service:Version");
        return ServiceVersion.Parse(value);
    }
}
