using PizzaAI;
using PizzaAI.Extensions;

// Load the environment variables (from .env file in development)
DotNetEnv.Env.Load();

// Create the builder
var builder = WebApplication.CreateBuilder(args);
// Charge automatically all custom webhosts settings
builder.WebHost.Initialize();
// Charge automatically all configuration managers
builder.Configuration.Initialize(builder.Environment);
// Charge automatically all custom services settings
builder.Services.Initialize(builder.Environment, builder.Configuration);

// Build the app
var app = builder.Build();
// Charge automatically all APP settings
app.Initialize(builder.Environment, builder.Configuration);
// Charge automatically all API settings
app.InitializeAPI(builder.Configuration);
// Run the app
app.Run();
