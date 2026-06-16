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

    public static IReadOnlyList<TEnum> ReadRequiredEnumList<TEnum>(IConfiguration configuration, string key)
        where TEnum : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var values = configuration.GetSection(key).Get<string[]>();
        if (values is null)
            throw new InvalidOperationException($"{key} is required.");

        return values
            .Select(value => Enum.Parse<TEnum>(value, ignoreCase: false))
            .Distinct()
            .ToList();
    }

    public static IReadOnlyList<TEnum> ReadOptionalEnumList<TEnum>(IConfiguration configuration, string key)
        where TEnum : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var values = configuration.GetSection(key).Get<string[]>();
        if (values is null)
            return [];

        return values
            .Select(value => Enum.Parse<TEnum>(value, ignoreCase: false))
            .Distinct()
            .ToList();
    }
}
