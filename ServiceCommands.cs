using Shared.GatewaySdk.ServicePublishing;

internal static class ServiceCommands
{
    public static ServiceCommandRegistry<ServiceCommandExecutor> Create(IConfiguration configuration)
    {
        var registry = new ServiceCommandRegistry<ServiceCommandExecutor>();
        foreach (var commandHead in configuration.GetSection("Service:Commands").Get<string[]>() ?? [])
        {
            registry.AddCommand(commandHead, "Service template command.")
                .AddExecutor(ServiceCommandExecutor.Default)
                .AddDescription("Handle service template command.")
                .ShowInPrompt();
        }

        return registry;
    }
}
