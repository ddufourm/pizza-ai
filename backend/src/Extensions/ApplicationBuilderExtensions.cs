using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

namespace PizzaAI.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication Initialize(this WebApplication app, IWebHostEnvironment env)
    {
        app.UseSecurityMiddlewarePipeline();
        app.UseCustomStaticFiles(env);
        app.MapSpaFallback(env);
        app.UseProductionSecurityHeaders(env);
        app.SetMapOpenApi(env);

        return app;
    }
    public static WebApplication UseCustomStaticFiles(this WebApplication app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment()) return app;
        var options = new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "wwwroot")),
            ContentTypeProvider = new FileExtensionContentTypeProvider
            {
                Mappings = {
                    [".webmanifest"] = "application/manifest+json",
                    [".wasm"] = "application/wasm"
                }
            }
        };
        app.UseStaticFiles(options);
        app.UseSecurityHeaders(env);

        return app;
    }

    public static IApplicationBuilder UseSecurityHeaders(this WebApplication app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment()) return app;
        return app.Use(async (context, next) =>
        {
            context.Response.Headers.Append("Content-Security-Policy", "default-src 'self' https: 'unsafe-inline' 'unsafe-eval'");
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            await next();
        });
    }

    public static IApplicationBuilder MapSpaFallback(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment()) return app;
        return app.MapWhen(context =>
            !context.Request.Path.StartsWithSegments("/api"),
            spa => spa.UseSpaFallback(env)
        );
    }

    private static IApplicationBuilder UseSpaFallback(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment()) return app;
        app.UseStaticFiles();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapFallbackToFile("index.html");
        });

        return app;
    }

    public static WebApplication UseProductionSecurityHeaders(this WebApplication app, IHostEnvironment env)
    {
        if (env.IsDevelopment()) return app;

        // Middleware pour forcer le schéma HTTPS
        app.Use(async (context, next) =>
        {
            context.Request.Scheme = "https";
            await next();
        });

        // Middleware pour les headers forwardés (X-Forwarded-For, etc.)
        app.UseForwardedHeaders();

        return app;
    }

    public static IApplicationBuilder UseSecurityMiddlewarePipeline(this WebApplication app)
    {
        return app
            .UseRouting()
            .UseCors("AllowAngular")
            .UseAuthentication()
            .UseAuthorization();
    }

    public static WebApplication SetMapOpenApi(this WebApplication app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            // Configure OpenAPI in development
            app.MapOpenApi();
        }
        return app;
    }

    public static IApplicationBuilder MapCorsPreflightRequests(this WebApplication app, string[] allowedOrigins)
    {
        return app.MapWhen(IsPreflightRequest, HandlePreflightRequests(allowedOrigins));
    }

    private static bool IsPreflightRequest(HttpContext context)
    {
        return context.Request.Method == "OPTIONS";
    }

    private static Action<IApplicationBuilder> HandlePreflightRequests(string[] origins)
    {
        return builder => builder.Run(async context =>
        {
            context.Response.StatusCode = 204;
            context.Response.Headers.Append("Access-Control-Allow-Origin", string.Join(",", origins));
            context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization");
            context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");

            await Task.CompletedTask;
        });
    }
}
