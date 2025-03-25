using PizzaAI;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography.X509Certificates;

[assembly: ApiController]

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(builder.Environment.IsDevelopment() ? 5205 : 8080);
    if (!builder.Environment.IsDevelopment())
    {
        serverOptions.Limits.MaxRequestBodySize = 52428800; // 50 MB
    }
});

if (!builder.Environment.IsDevelopment())
{
    builder.Configuration.AddKeyPerFile("/run/secrets", optional: true, reloadOnChange: true);

    // Forcer le chargement de la configuration HTTPS
    builder.Services.AddHttpsRedirection(options =>
    {
        options.HttpsPort = 443;
    });

    try
    {
        var certPath = Environment.GetEnvironmentVariable("CERT_PATH");
        var certPassword = builder.Configuration["CERT_PASSWORD"];
        var certificate = X509Certificate2.CreateFromEncryptedPemFile(
            certPath!,
            certPassword!
        );

        builder.Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo("/app/data-protection-keys"))
            .ProtectKeysWithCertificate(certificate)
            .SetApplicationName("PizzaAI")
            .SetDefaultKeyLifetime(TimeSpan.FromDays(90));
    }
    catch (Exception e)
    {
        Console.WriteLine($"Erreur lors de la configuration du certificat : {e.Message}");

        // Configurer une protection des données de base sans certificat
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

// Configuration des services d'authentification
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();
builder.Services.AddAuthorization();

if (!builder.Environment.IsDevelopment())
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });
}

// Add services to the container.
builder.Services.AddOpenApi();

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
            .AllowCredentials() // Ajout crucial pour les requêtes authentifiées
            .SetPreflightMaxAge(TimeSpan.FromSeconds(1800)) // Cache des pré-vérifications
            .WithExposedHeaders("Content-Disposition"); // Pour les téléchargements de fichiers
    });
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true; // Désactive la validation automatique si gérée manuellement
});

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
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

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseForwardedHeaders();
}
app.UseRouting();
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();

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


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();
}

app.UseAIRequest(builder.Configuration);
app.Run();
