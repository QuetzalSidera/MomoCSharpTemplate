using Shared.Contracts.StandardEvents;
using Shared.GatewaySdk.ServicePublishing;

internal static class EventHandler
{
    public static EventHandlerResult Handle(ServiceEventEnvelope envelope,
        ServiceCommandRegistry<ServiceCommandExecutor> commandRegistry)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        ArgumentNullException.ThrowIfNull(commandRegistry);

        if (envelope.Payload is SubscriberMessageEventPayload message)
        {
            var resolution = commandRegistry.Resolve(message.RawMessage);
            if (resolution is ServiceCommandResolution<ServiceCommandExecutor>.MatchedResult matched)
                return matched.Match.Executor.Execute(envelope, matched.Match);
        }

        var toolCallResult = ServiceToolCallDecisionHandler.TryHandle(envelope, commandRegistry);
        if (toolCallResult is not null)
            return toolCallResult;

        return EventHandlerResult.Ignored("No local command or ToolCall decision matched.");
    }
}
