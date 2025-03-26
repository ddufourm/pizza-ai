namespace PizzaAI.Extensions;

public static class CorsExtensions
{
    public static IServiceCollection AddCustomCors(this IServiceCollection services)
    {
        return services.AddCors(options =>
        {
            // Add the CORS policy
            var corsOrigins = Environment.GetEnvironmentVariable("CORS_ORIGINS")?.Split(',') ?? [];
            if (corsOrigins.Length == 0)
            {
                throw new InvalidOperationException("Aucune origine CORS configurée !");
            }

            options.AddPolicy("AllowAngular", policy =>
            {
                policy.WithOrigins(corsOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials() // Crucial addition for authenticated requests
                    .SetPreflightMaxAge(TimeSpan.FromSeconds(1800)) // Pre-check cover
                    .WithExposedHeaders("Content-Disposition"); // For file downloads
            });
        });
    }
}
