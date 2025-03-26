using PizzaAI;
using PizzaAI.Extensions;
using System.Reflection;

// Load the environment variables (from .env file in development)
DotNetEnv.Env.Load();

// Create the builder
var builder = WebApplication.CreateBuilder(args);

// Charge automatically all the extensions
builder.Services.AddAllExtensions(Assembly.GetExecutingAssembly());

// Build the app
var app = builder.Build();

// Add the AI request middleware
app.UseAIRequest(builder.Configuration);

// Run the app
app.Run();
