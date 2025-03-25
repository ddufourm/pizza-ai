using PizzaAI;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography.X509Certificates;

[assembly: ApiController]

// Load the environment variables (from .env file in development)
DotNetEnv.Env.Load();

// Create the builder
var builder = WebApplication.CreateBuilder(args);

// Load the secrets from the Docker secrets in production
if (!builder.Environment.IsDevelopment())
{
    builder.Configuration.AddKeyPerFile("/run/secrets", optional: true, reloadOnChange: true);
}

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    // Listen on all interfaces on the specified port
    // Listen on all interfaces on the specified port
    serverOptions.ListenAnyIP(int.Parse(Environment.GetEnvironmentVariable("PORT") ?? "5205"), listenOptions =>
    {
        // Use HTTPS in production
        if (!builder.Environment.IsDevelopment())
        {
            listenOptions.UseHttps("/app/certs/certificate.pfx", builder.Configuration["CERT_PASSWORD"] ?? throw new InvalidOperationException("Aucun mot de passe certificat configuré en production !"));
        }
    });

    // Limit the request body size to 50 MB in production
    if (!builder.Environment.IsDevelopment())
    {
        serverOptions.Limits.MaxRequestBodySize = 52428800; // 50 MB
    }
});

// Add https informations in production
if (!builder.Environment.IsDevelopment())
{
    // Force charging the HTTPS redirection middleware
    builder.Services.AddHttpsRedirection(options =>
    {
        options.HttpsPort = 443;
    });

    try
    {
        // Load the certificate from the file
        var certPath = Environment.GetEnvironmentVariable("CERT_PATH");
        var certPassword = builder.Configuration["CERT_PASSWORD"];
        var certificate = X509Certificate2.CreateFromEncryptedPemFile(
            certPath!,
            certPassword!
        );

        // Configure the data protection with the certificate
        builder.Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo("/app/data-protection-keys"))
            .ProtectKeysWithCertificate(certificate)
            .SetApplicationName("PizzaAI")
            .SetDefaultKeyLifetime(TimeSpan.FromDays(90));
    }
    catch (Exception e)
    {
        Console.WriteLine($"Error during certificate configuration : {e.Message}");

        // Set the default data protection configuration without certificate
        builder.Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo("/app/data-protection-keys"))
            .SetApplicationName("PizzaAI")
            .SetDefaultKeyLifetime(TimeSpan.FromDays(90));
    }
}

// Configure the HTTP request pipeline.
builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.PropertyNameCaseInsensitive = true;
    options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// Set the authentication and authorization services
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();
builder.Services.AddAuthorization();
// Add the x-forwarded-proto headers in production
if (!builder.Environment.IsDevelopment())
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });
}

// Add OpenAPI service to the container.
builder.Services.AddOpenApi();

// Add the CORS policy
var corsOrigins = Environment.GetEnvironmentVariable("CORS_ORIGINS")?.Split(',') ?? [];
if (corsOrigins.Length == 0)
{
    throw new InvalidOperationException("Aucune origine CORS configurée !");
}
builder.Services.AddCors(options =>
{
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

// Disables automatic validation if managed manually
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

// Set a return format for validation errors for API
builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
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
});

// Set the JSON serialization options
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});

// Build the app
var app = builder.Build();

// Configure the HTTP request pipeline in production.
if (!app.Environment.IsDevelopment())
{
    app.UseForwardedHeaders();
}
// Add Routing, the CORS policy and the authentication and authorization middlewares
app.UseRouting();
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();

// Add the Options method for CORS pre-checks
app.MapWhen(context => context.Request.Method == "OPTIONS", handleOptions =>
{
    handleOptions.Run(async context =>
    {
        context.Response.StatusCode = 204;
        context.Response.Headers.AccessControlAllowOrigin = string.Join(",", corsOrigins);
        context.Response.Headers.AccessControlAllowHeaders = "Content-Type, Authorization";
        context.Response.Headers.AccessControlAllowMethods = "GET, POST, PUT, DELETE, OPTIONS";
        await Task.CompletedTask; // Pas de next() car c'est une réponse terminale
    });
});

if (app.Environment.IsDevelopment())
{
    // Configure OpenAPI in development
    app.MapOpenApi();
}

// Add the AI request middleware
app.UseAIRequest(builder.Configuration);

// Run the app
app.Run();
