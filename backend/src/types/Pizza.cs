public class Pizza
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Size { get; set; }
    public decimal? Price { get; set; }
    public string? Emotion { get; set; }
    public string? Image { get; set; }

    public Pizza(string id, string name, string size, decimal price, string emotion, string image)
    {
        Id = id;
        Name = name;
        Size = size;
        Price = price;
        Emotion = emotion;
        Image = image;
    }

    // Parameterless constructor
    public Pizza()
    {
    }
}
