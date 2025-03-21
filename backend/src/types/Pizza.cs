public class Pizza
{
    public string Id { get; set; }
    public string Nom { get; set; }
    public string Grandeur { get; set; }
    public decimal Prix { get; set; }
    public string Emotion { get; set; }

    public Pizza(string id, string nom, string grandeur, decimal prix, string emotion)
    {
        Id = id;
        Nom = nom;
        Grandeur = grandeur;
        Prix = prix;
        Emotion = emotion;
    }

    // Parameterless constructor
    public Pizza()
    {
    }
}
