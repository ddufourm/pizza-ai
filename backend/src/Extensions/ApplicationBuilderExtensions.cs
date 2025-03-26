using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

namespace PizzaAI.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseCustomStaticFiles(this IApplicationBuilder app, IWebHostEnvironment env)
    {
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

        return app.UseStaticFiles(options).UseSecurityHeaders();
    }

    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            context.Response.Headers.Append("Content-Security-Policy", "default-src 'self' https: 'unsafe-inline' 'unsafe-eval'");
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            await next();
        });
    }

    public static IApplicationBuilder MapSpaFallback(
        this IApplicationBuilder app)
    {
        return app.MapWhen(context =>
            !context.Request.Path.StartsWithSegments("/api"),
            spa => spa.UseSpaFallback()
        );
    }

    private static IApplicationBuilder UseSpaFallback(this IApplicationBuilder app)
    {
        app.UseStaticFiles();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapFallbackToFile("index.html");
        });

        return app;
    }

    public static IApplicationBuilder UseProductionSecurityHeaders(this IApplicationBuilder app, IHostEnvironment env)
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

    public static IApplicationBuilder UseSecurityMiddlewarePipeline(this IApplicationBuilder app)
    {
        return app
            .UseRouting()
            .UseCors("AllowAngular")
            .UseAuthentication()
            .UseAuthorization();
    }

    public static IApplicationBuilder SetMapOpenApi(this WebApplication app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            // Configure OpenAPI in development
            app.MapOpenApi();
        }
        return app;
    }
}
