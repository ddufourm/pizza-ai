using System.Text;
using System.Text.Json;

using PizzaAI.types;

namespace PizzaAI
{
    public interface IOpenAIService
    {
        Task<OpenAI> GetPizzaSuggestion(string inputText);
    }

    public class OpenAIService : IOpenAIService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiKey;

        public OpenAIService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _apiKey = Environment.GetEnvironmentVariable("OPENAI_KEY") ?? configuration["OPENAI_KEY"] ?? throw new InvalidOperationException("API_KEY environment variable is not set.");
        }

        public async Task<OpenAI> GetPizzaSuggestion(string inputText)
        {
            // Create a new HttpClient instance and add the Authorization header
            using var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

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
                    new { role = "user", content = inputText }
                }
            };

            var response = await client.PostAsync(
                "https://api.openai.com/v1/chat/completions",
                new StringContent(JsonSerializer.Serialize(openAiRequest), Encoding.UTF8, "application/json")
            );

            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<OpenAI>(responseBody) ?? throw new Exception("Réponse OpenAI invalide");
        }
    }
}
