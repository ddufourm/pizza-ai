

public static class ConfigurationManagerExtensions
{
    public static IConfigurationBuilder Initialize(this IConfigurationManager configuration, IWebHostEnvironment env)
    {
        configuration.AddKeyPerFileSecrets(env);
        return configuration;
    }
    public static IConfigurationBuilder AddKeyPerFileSecrets(this IConfigurationManager configuration, IWebHostEnvironment env)
    {
        if (env.IsDevelopment()) return configuration;
        return configuration.AddKeyPerFile("/run/secrets", optional: true, reloadOnChange: true);
    }
}
