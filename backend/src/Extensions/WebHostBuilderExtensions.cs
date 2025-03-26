namespace PizzaAI.Extensions;

public static class WebHostBuilderExtensions
{
    public static IWebHostBuilder ConfigureKestrelServer(this IWebHostBuilder builder)
    {
        return builder.ConfigureKestrel(serverOptions =>
        {
            // Listen on all interfaces on the specified port
            serverOptions.ListenAnyIP(int.Parse(Environment.GetEnvironmentVariable("PORT") ?? "5205"));
            // Limit the request body size to 50 MB in production
            serverOptions.Limits.MaxRequestBodySize = 52428800; // 50 MB
        });
    }
}
