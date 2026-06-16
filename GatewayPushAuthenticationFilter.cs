using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Shared.GatewaySdk;
using Shared.GatewaySdk.Http;

internal sealed class GatewayPushAuthenticationFilter(IOptions<GatewayClientOptions> options) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var expected = options.Value.GatewayPushSecret;
        if (string.IsNullOrWhiteSpace(expected))
            throw new InvalidOperationException("Gateway:GatewayPushSecret is required.");

        var actual = ResolveCredential(context.HttpContext.Request);
        if (actual is null || !FixedEquals(expected, actual))
            return Results.Unauthorized();

        return await next(context);
    }

    private static string? ResolveCredential(HttpRequest request)
    {
        var authorization = request.Headers.Authorization.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(authorization) &&
            authorization.StartsWith(GatewayHttpAuthorization.BearerPrefix, StringComparison.OrdinalIgnoreCase))
            return Normalize(authorization[GatewayHttpAuthorization.BearerPrefix.Length..]);

        return request.Headers.TryGetValue(GatewayHttpHeaderNames.GatewayPushSecret, out var values)
            ? Normalize(values.FirstOrDefault())
            : null;
    }

    private static bool FixedEquals(string expected, string actual)
    {
        var expectedBytes = Encoding.UTF8.GetBytes(expected.Trim());
        var actualBytes = Encoding.UTF8.GetBytes(actual.Trim());
        return expectedBytes.Length == actualBytes.Length &&
               CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
    }

    private static string? Normalize(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}
