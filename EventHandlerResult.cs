using Shared.Contracts.StandardEvents;
using Shared.GatewaySdk.Messages;

internal sealed record EventHandlerResult
{
    private EventHandlerResult(ServiceEventAck ack, IReadOnlyList<GatewayMessageSendIntent> sendIntents)
    {
        Ack = ack;
        SendIntents = sendIntents;
    }

    public ServiceEventAck Ack { get; }

    public IReadOnlyList<GatewayMessageSendIntent> SendIntents { get; }

    public static EventHandlerResult Handled(IReadOnlyList<GatewayMessageSendIntent>? sendIntents = null)
    {
        return new EventHandlerResult(ServiceEventAck.Succeeded, sendIntents ?? []);
    }

    public static EventHandlerResult Ignored(string? message = null)
    {
        return new EventHandlerResult(new ServiceEventAck
        {
            Status = ServiceEventAckStatus.Ignored,
            DiagnosticMessage = message
        }, []);
    }
}
