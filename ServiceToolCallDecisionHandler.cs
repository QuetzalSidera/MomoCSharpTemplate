using Shared.Contracts.StandardEvents;
using Shared.GatewaySdk.ServicePublishing;

internal static class ServiceToolCallDecisionHandler
{
    public static EventHandlerResult? TryHandle(ServiceEventEnvelope envelope,
        ServiceCommandRegistry<ServiceCommandExecutor> commandRegistry)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        ArgumentNullException.ThrowIfNull(commandRegistry);

        return ToolCallDecisionCommandResolver.TryResolve(envelope.Metadata, commandRegistry, out var match)
            ? match.Executor.Execute(envelope, match)
            : null;
    }
}
