namespace PizzaAI.Extensions;

public static class CorsPreflightExtensions
{
    public static IApplicationBuilder MapCorsPreflightRequests(
        this IApplicationBuilder app,
        string[] allowedOrigins)
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
