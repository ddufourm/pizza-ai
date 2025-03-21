using PizzaAI;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Negotiate;

[assembly: ApiController]

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

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

// Add services to the container.
builder.Services.AddOpenApi();

var corsOrigins = builder.Environment.IsDevelopment()
    ? Environment.GetEnvironmentVariable("CORS_ORIGINS_DEV")?.Split(',')
    : Environment.GetEnvironmentVariable("CORS_ORIGINS_PROD")?.Split(',');
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(corsOrigins ?? Array.Empty<string>())
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials() // Ajout crucial pour les requêtes authentifiées
            .SetPreflightMaxAge(TimeSpan.FromSeconds(1800)); // Cache des pré-vérifications
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
                .Where(e => e.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
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
app.UseRouting();
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();
}

app.UseAIRequest();
app.Run();
