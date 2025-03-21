namespace PizzaAI.types
{
    public class OpenAI
    {
        public List<Choice> choices { get; set; } = new();
    }

    public class Choice
    {
        public Message message { get; set; } = new();
    }

    public class Message
    {
        public string content { get; set; } = string.Empty;
    }
}
