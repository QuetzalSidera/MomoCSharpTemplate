using Shared.Contracts.Capabilities;

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

    public static IReadOnlyList<ICoreUpStreamCapabilityConfig> ReadRequiredUpStreamCapabilityList(
        IConfiguration configuration,
        string key)
    {
        return ReadRequiredCapabilitySpecs(configuration, key)
            .Select(spec => CoreCapabilityConfigParser.ParseUpStream(spec.Domain, spec.Dimensions))
            .Distinct()
            .ToList();
    }

    public static IReadOnlyList<ICoreDownStreamCapabilityConfig> ReadOptionalDownStreamCapabilityList(
        IConfiguration configuration,
        string key)
    {
        return ReadOptionalCapabilitySpecs(configuration, key)
            .Select(spec => CoreCapabilityConfigParser.ParseDownStream(spec.Domain, spec.Dimensions))
            .Distinct()
            .ToList();
    }

    private static IReadOnlyList<ConfiguredCapability> ReadRequiredCapabilitySpecs(IConfiguration configuration,
        string key)
    {
        var values = ReadOptionalCapabilitySpecs(configuration, key);
        if (values.Count == 0)
            throw new InvalidOperationException($"{key} is required.");

        return values;
    }

    private static IReadOnlyList<ConfiguredCapability> ReadOptionalCapabilitySpecs(IConfiguration configuration,
        string key)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        return configuration.GetSection(key).Get<ConfiguredCapability[]>()?
            .Where(value => !string.IsNullOrWhiteSpace(value.Domain))
            .ToList() ?? [];
    }

    private sealed record ConfiguredCapability
    {
        public string Domain { get; init; } = string.Empty;

        public Dictionary<string, string> Dimensions { get; init; } = [];
    }
}
