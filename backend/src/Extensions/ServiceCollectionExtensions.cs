using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.HttpOverrides;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace PizzaAI.Extensions;

public static class ServiceCollectionExtensions
{
    private static readonly HashSet<string> InvokedMethods = new();
    public static IServiceCollection AddAllExtensions(this IServiceCollection services, Assembly assembly)
    {
        // Récupérer toutes les classes statiques dans l'assembly spécifié
        var extensionMethods = assembly.GetTypes()
            .Where(type => type.IsClass && type.IsAbstract && type.IsSealed) // Classes statiques
            .SelectMany(type => type.GetMethods(BindingFlags.Static | BindingFlags.Public))
            .Where(method => method.GetParameters().FirstOrDefault()?.ParameterType == typeof(IServiceCollection) &&
            method.Name != nameof(AddAllExtensions)).ToList();

        foreach (var method in extensionMethods)
        {
            if (InvokedMethods.Contains(method.Name))
            {
                Console.WriteLine($"Méthode déjà appelée : {method.Name}");
                continue;
            }

            Console.WriteLine($"Appel de la méthode d'extension : {method.Name}");
            // Vérifier si la méthode est une extension de IServiceCollection
            if (method.ReturnType == typeof(IServiceCollection))
            {
                try
                {
                    // Appeler la méthode d'extension dynamiquement
                    method.Invoke(null, new object[] { services });
                    InvokedMethods.Add(method.Name);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de l'appel de la méthode '{method.Name}': {ex.Message}");
                }
            }
        }

        return services;
    }

    public static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        return services
            .AddControllers()
            .AddApplicationPart(typeof(Program).Assembly)
            .Services;
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

    public static IServiceCollection AddOpenApi(this IServiceCollection services)
    {
        return services.AddOpenApi();
    }

    public static IServiceCollection ConfigureForwardedHeadersOptions(this IServiceCollection services, WebApplicationBuilder builder)
    {
        if (builder.Environment.IsDevelopment()) return services;

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

public class CorsSettings
{
    public required string[] AllowedOrigins { get; set; }
}
