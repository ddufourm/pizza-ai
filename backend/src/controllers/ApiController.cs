namespace PizzaAI
{
    public static class Api
    {
        public static WebApplication InitializeAPI(this WebApplication app, IConfiguration configuration)
        {
            // Add the AI request middleware
            app.UseAIRequest(configuration);

            return app;
        }
    }
}
