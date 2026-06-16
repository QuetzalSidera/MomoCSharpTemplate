using Shared.Contracts.MessageSegments;
using Shared.Contracts.MessageSegments.Derived;
using Shared.Contracts.StandardEvents;
using Shared.GatewaySdk.Messages;
using Shared.GatewaySdk.ServicePublishing;

internal sealed record ServiceCommandExecutor
{
    public static ServiceCommandExecutor Default { get; } = new();

    public EventHandlerResult Execute(ServiceEventEnvelope envelope,
        ServiceCommandMatch<ServiceCommandExecutor> match)
    {
        Segment segment = TextSegment.NewText(
            $"Handled command {match.CommandHead} with {match.Arguments.Count} argument(s).");
        return EventHandlerResult.Handled([
            GatewayMessageSendIntent.Passive(segment, $"{envelope.EventTraceId:N}:command")
        ]);
    }
}
