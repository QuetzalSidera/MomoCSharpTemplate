using Shared.Contracts.Capabilities;
using Shared.Contracts.Policies;

internal static class ServiceTemplateConfiguration
{
    public static void ValidateStartup(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        ReadRequiredString(configuration, "Gateway:GatewayPushSecret");
        ReadRequiredString(configuration, "Gateway:GatewayPushEncryptionKey");
    }

    public static string ReadRequiredString(IConfiguration configuration, string key)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var value = configuration.GetValue<string>(key);
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"{key} is required.");

        return value.Trim();
    }

    public static IReadOnlyList<IEventCapabilityConfig> ReadConsumedEventCapabilityList(
        IConfiguration configuration,
        string key)
    {
        return ReadRequiredCapabilityIdentifiers(configuration, key)
            .Select(CapabilityConfigs.Events.Event)
            .Distinct()
            .ToList();
    }

    public static IReadOnlyList<IActionCapabilityConfig> ReadActionCapabilityList(
        IConfiguration configuration,
        string key)
    {
        return ReadOptionalCapabilityIdentifiers(configuration, key)
            .Select(CapabilityConfigs.Actions.Action)
            .Distinct()
            .ToList();
    }

    public static IReadOnlyList<GatewayPolicyType> ReadGatewayPolicyTypeList(
        IConfiguration configuration,
        string key)
    {
        return ReadOptionalCapabilityIdentifiers(configuration, key)
            .Select(GatewayPolicyRegistry.Parse)
            .Distinct()
            .ToList();
    }

    private static IReadOnlyList<string> ReadRequiredCapabilityIdentifiers(IConfiguration configuration,
        string key)
    {
        var values = ReadOptionalCapabilityIdentifiers(configuration, key);
        if (values.Count == 0)
            throw new InvalidOperationException($"{key} is required.");

        return values;
    }

    private static IReadOnlyList<string> ReadOptionalCapabilityIdentifiers(IConfiguration configuration,
        string key)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        return configuration.GetSection(key).Get<string[]>()?
            .Select(value => value.Trim())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .ToList() ?? [];
    }
}
