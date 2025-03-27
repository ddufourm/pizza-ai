using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.HttpOverrides;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography.X509Certificates;

namespace PizzaAI.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection Initialize(
        this IServiceCollection services,
        IWebHostEnvironment environment,
        IConfiguration config
    )
    {
        services.AddCustomCors();
        services.AddSecureDataProtection(environment, config);
        services.AddCustomAuthentication();
        services.AddCustomuthorization();
        services.InitializeOpenApi();
        services.ConfigureForwardedHeadersOptions(environment);
        services.ConfigureCustomJsonOptions();
        services.ConfigureApiBehaviorOptions();
        services.ConfigureCustomApiBehavior();
        services.AddJsonOptions();
        return services;
    }

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

    public static IDataProtectionBuilder AddSecureDataProtection(
        this IServiceCollection services,
        IWebHostEnvironment env,
        IConfiguration config)
    {
        // In development, use the default data protection configuration
        if (env.IsDevelopment())
        {
            return services.AddDataProtection()
                .SetApplicationName("PizzaAI")
                .SetDefaultKeyLifetime(TimeSpan.FromDays(90));
        }

        try
        {
            // Load the certificate from the file
            var certPath = Environment.GetEnvironmentVariable("CERT_PATH");
            var certPassword = config["CERT_PASSWORD"];
            var certificate = X509CertificateLoader.LoadPkcs12FromFile(
                Path.Combine(certPath!, "certificate.pfx"),
                certPassword!,
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.EphemeralKeySet
            );

            // Configure the data protection with the certificate
            return services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo("/app/data-protection-keys"))
                .ProtectKeysWithCertificate(certificate)
                .SetApplicationName("PizzaAI")
                .SetDefaultKeyLifetime(TimeSpan.FromDays(90));
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error during certificate configuration : {e.Message}");

            // Set the default data protection configuration without certificate
            return services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo("/app/data-protection-keys"))
                .SetApplicationName("PizzaAI")
                .SetDefaultKeyLifetime(TimeSpan.FromDays(90));
        }
    }

    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services)
    {
        return services
            .AddAuthentication(NegotiateDefaults.AuthenticationScheme)
            .AddNegotiate().Services;
    }

    public static IServiceCollection AddCustomuthorization(this IServiceCollection services)
    {
        return services.AddAuthorization();
    }

    public static IServiceCollection InitializeOpenApi(this IServiceCollection services)
    {
        return services.AddOpenApi();
    }

    public static IServiceCollection ConfigureForwardedHeadersOptions(this IServiceCollection services, IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment()) return services;

        // Configure the Forwarded Headers Middleware       
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        return services;
    }

    public static IServiceCollection ConfigureCustomJsonOptions(this IServiceCollection services)
    {
        services.Configure<JsonSerializerOptions>(options =>
        {
            options.PropertyNameCaseInsensitive = true; // Ignorer la casse des noms de propriétés
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull; // Ignorer les valeurs nulles
        });

        return services;
    }

    public static IServiceCollection ConfigureApiBehaviorOptions(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        return services;
    }

    public static IServiceCollection ConfigureCustomApiBehavior(this IServiceCollection services)
    {
        return services.AddControllers().ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(e => e.Value?.Errors?.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors?.Select(e => e.ErrorMessage).ToArray() ?? []
                    );

                return new BadRequestObjectResult(new
                {
                    Code = "VALIDATION_ERROR",
                    Message = "Erreur de validation des données",
                    Errors = errors
                });
            };
        }).Services;
    }

    public static IServiceCollection AddJsonOptions(this IServiceCollection services)
    {
        return services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = null;
        }).Services;
    }
}
