public class HstsExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;

    public HstsExceptionHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception)
        {
            context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
            throw;
        }
    }
}

public static class HstsExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseHstsExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<HstsExceptionHandlerMiddleware>();
    }
}
