using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

using PizzaAI.types;

namespace PizzaAI
{
    public static class AIRequest
    {
        public static WebApplication UseAIRequest(this WebApplication app, IConfiguration configuration)
        {
            app.MapPost("/api/pizza/pizza-suggestion", async ([FromBody] PizzaRequest request) =>
            {
                try
                {
                    // Get the API key from the environment variables
                    string apiKey = Environment.GetEnvironmentVariable("OPENAI_KEY") ?? configuration["OPENAI_KEY"] ?? throw new InvalidOperationException("API_KEY environment variable is not set.");
                    // Create a new HttpClient instance and add the Authorization header
                    using var client = new HttpClient();
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                    // Create the request body for the OpenAI API
                    var pizzaListContent = Pizzas.GeneratePizzaList();
                    var systemMessage = $@"
                        Vous êtes un expert en pizzas qui suggère des pizzas basées sur les émotions. Suivez strictement ces règles :

                        1. Répondez UNIQUEMENT avec le numéro (ID) de la pizza suggérée.
                        2. Choisissez TOUJOURS une pizza de taille Petite, sauf si l'utilisateur mentionne explicitement être accompagné.
                        3. Ignorez toute préférence de goût mentionnée.
                        4. Variez vos suggestions en fonction des émotions exprimées.
                        5. Associez chaque émotion à une pizza différente pour maximiser la diversité des suggestions.

                        Voici la liste des pizzas disponibles :

                        {pizzaListContent}
                        Répondez uniquement avec le numéro correspondant à la pizza choisie. Par exemple : 1 ou 5 ou 9, etc.
                        ";
                    var openAiRequest = new
                    {
                        model = "gpt-3.5-turbo",
                        messages = new[]{
                            new { role = "system", content = systemMessage },
                            new { role = "user", content = request.Text }
                        }
                    };
                    // Send the request to the OpenAI API
                    var response = await client.PostAsync(
                        "https://api.openai.com/v1/chat/completions",
                        new StringContent(JsonSerializer.Serialize(openAiRequest), Encoding.UTF8, "application/json")
                    );
                    // Read the response from the OpenAI API
                    var responseBody = await response.Content.ReadAsStringAsync();
                    // Deserialize the response from the OpenAI API
                    OpenAI openAIResponse = JsonSerializer.Deserialize<OpenAI>(responseBody) ?? throw new Exception("Réponse OpenAI invalide");
                    // Get the pizza suggestion ID from the response
                    var pizzaSuggestionId = openAIResponse.choices.FirstOrDefault()?.message?.content ?? "Aucune suggestion";
                    // Extract the pizza ID from the suggestion
                    Match match = Regex.Match(pizzaSuggestionId, @"\d+");
                    if (!match.Success)
                    {
                        throw new InvalidOperationException("Aucun match trouvé.");
                    }
                    int number = int.Parse(match.Value);
                    // Get the pizza suggestion by ID
                    var suggestion = Pizzas.GetPizzaById(number.ToString());
                    // Return the pizza suggestion
                    return Results.Ok(new
                    {
                        success = true,
                        pizza = suggestion
                    });
                }
                catch (Exception ex)
                {
                    // Return a problem response if an error occurs
                    return Results.Problem(new ProblemDetails
                    {
                        Status = 500,
                        Title = "Une erreur est survenue",
                        Detail = ex.Message
                    });
                }
            })
            .RequireCors("AllowAngular")
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status200OK)
            .WithOpenApi();
            //.RequireAuthorization(); // Enforce authorization here

            return app;
        }
    }

    // Define the request body for the pizza suggestion endpoint
    public class PizzaRequest
    {
        [Required]
        public string Text { get; set; } = "";
        [Required]
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
