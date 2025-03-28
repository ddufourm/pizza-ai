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
            app.MapPost("/api/pizza/pizza-suggestion", async ([FromBody] PizzaRequest request, [FromServices] IOpenAIService openAIService) =>
            {
                try
                {
                    OpenAI openAIResponse = await openAIService.GetPizzaSuggestion(request.Text);

                    var pizzaSuggestionId = openAIResponse.choices.FirstOrDefault()?.message?.content ?? "Aucune suggestion";
                    Match match = Regex.Match(pizzaSuggestionId, @"\d+");
                    if (!match.Success)
                    {
                        throw new InvalidOperationException("Aucun match trouvé.");
                    }
                    int number = int.Parse(match.Value);

                    var suggestion = Pizzas.GetPizzaById(number.ToString());

                    return Results.Ok(new
                    {
                        success = true,
                        pizza = suggestion
                    });
                }
                catch (Exception ex)
                {
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
