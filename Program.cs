using Microsoft.Extensions.Options;
using Shared.Contracts.StandardEvents;
using Shared.GatewaySdk;
using Shared.GatewaySdk.Http;
using Shared.GatewaySdk.Messages;
using Shared.GatewaySdk.ServicePublishing;

namespace ServiceTemplate;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        ServiceTemplateConfiguration.ValidateStartup(builder.Configuration);

        builder.Services.AddAuthorization();
        builder.Services.AddOpenApi();
        builder.Services.AddGatewayMessageClient(builder.Configuration);
        builder.Services.AddGatewayServicePublishClient(builder.Configuration);
        builder.Services.AddSingleton(provider =>
            ServiceCommands.Create(provider.GetRequiredService<IConfiguration>()));
        builder.Services.AddHostedService<ServicePublishHostedService>();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseAuthorization();

        app.MapGet(MicroserviceHttpRoutes.HealthAbsolutePath, () => Results.Ok(new { status = "Healthy" }))
            .WithName("Health");

        app.MapPost(MicroserviceHttpRoutes.EventAbsolutePath, async (GatewayServiceEventPushRequest request,
                GatewayMessageClient gateway,
                ServiceCommandRegistry<ServiceCommandExecutor> commandRegistry,
                IOptions<GatewayClientOptions> gatewayOptions,
                CancellationToken cancellationToken) =>
            {
                var envelope = GatewayServiceEventPayloadProtector.Unprotect(request,
                    gatewayOptions.Value.GatewayPushEncryptionKey);
                var result = EventHandler.Handle(envelope, commandRegistry);
                await gateway.DispatchAsync(envelope, result.SendIntents, cancellationToken);
                return TypedResults.Ok(result.Ack);
            })
            .AddEndpointFilter<GatewayPushAuthenticationFilter>()
            .WithName("HandleGatewayEvent");

        app.Run();
    }
}
