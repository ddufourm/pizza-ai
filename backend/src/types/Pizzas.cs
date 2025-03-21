namespace PizzaAI.types
{
    public class Pizzas
    {
        public static List<Pizza> pizzas = new List<Pizza>
        {
            new Pizza("1", "Margherita", "Petite", 10.99m, "pour des émotions neutres ou joyeuses"),
            new Pizza("2", "Margherita", "Moyenne", 12.99m, "pour des émotions neutres ou joyeuses"),
            new Pizza("3", "Margherita", "Grande", 14.99m, "pour des émotions neutres ou joyeuses"),
            new Pizza("4", "Margherita", "Très Grande", 16.99m, "pour des émotions neutres ou joyeuses"),
            new Pizza("5", "Quattro Formaggi", "Petite", 13.50m, "pour le réconfort ou la nostalgie"),
            new Pizza("6", "Quattro Formaggi", "Moyenne", 15.50m, "pour le réconfort ou la nostalgie"),
            new Pizza("7", "Quattro Formaggi", "Grande", 17.50m, "pour le réconfort ou la nostalgie"),
            new Pizza("8", "Quattro Formaggi", "Très Grande", 19.50m, "pour le réconfort ou la nostalgie"),
            new Pizza("9", "Pepperoni", "Petite", 11.75m, "pour l'excitation ou l'enthousiasme"),
            new Pizza("10", "Pepperoni", "Moyenne", 13.75m, "pour l'excitation ou l'enthousiasme"),
            new Pizza("11", "Pepperoni", "Grande", 15.75m, "pour l'excitation ou l'enthousiasme"),
            new Pizza("12", "Pepperoni", "Très Grande", 17.75m, "pour l'excitation ou l'enthousiasme"),
            new Pizza("13", "Provençale", "Petite", 12.25m, "pour la sérénité ou la détente"),
            new Pizza("14", "Provençale", "Moyenne", 14.25m, "pour la sérénité ou la détente"),
            new Pizza("15", "Provençale", "Grande", 16.25m, "pour la sérénité ou la détente"),
            new Pizza("16", "Provençale", "Très Grande", 18.25m, "pour la sérénité ou la détente"),
            new Pizza("17", "Capricciosa", "Petite", 13.00m, "pour la curiosité ou l'aventure"),
            new Pizza("18", "Capricciosa", "Moyenne", 15.00m, "pour la curiosité ou l'aventure"),
            new Pizza("19", "Capricciosa", "Grande", 17.00m, "pour la curiosité ou l'aventure"),
            new Pizza("20", "Capricciosa", "Très Grande", 19.00m, "pour la curiosité ou l'aventure"),
            new Pizza("21", "Hawaienne", "Petite", 11.99m, "pour la surprise ou l'optimisme"),
            new Pizza("22", "Hawaienne", "Moyenne", 13.99m, "pour la surprise ou l'optimisme"),
            new Pizza("23", "Hawaienne", "Grande", 15.99m, "pour la surprise ou l'optimisme"),
            new Pizza("24", "Hawaienne", "Très Grande", 17.99m, "pour la surprise ou l'optimisme"),
            new Pizza("25", "Quatre Saisons", "Petite", 14.50m, "pour des émotions complexes ou changeantes"),
            new Pizza("26", "Quatre Saisons", "Moyenne", 16.50m, "pour des émotions complexes ou changeantes"),
            new Pizza("27", "Quatre Saisons", "Grande", 18.50m, "pour des émotions complexes ou changeantes"),
            new Pizza("28", "Quatre Saisons", "Très Grande", 20.50m, "pour des émotions complexes ou changeantes"),
            new Pizza("29", "Calzone", "Petite", 12.75m, "pour l'introspection ou le besoin de protection"),
            new Pizza("30", "Calzone", "Moyenne", 14.75m, "pour l'introspection ou le besoin de protection"),
            new Pizza("31", "Calzone", "Grande", 16.75m, "pour l'introspection ou le besoin de protection"),
            new Pizza("32", "Calzone", "Très Grande", 18.75m, "pour l'introspection ou le besoin de protection")
        };

        // Generate a list of pizzas for the OpenAI request
        public static string GeneratePizzaList()
        {
            var pizzaList = string.Join("\n", pizzas.Select((p, i) =>
                $"{p.Id}. {p.Nom} {p.Grandeur} - {p.Emotion}"));
            return pizzaList;
        }

        public static Pizza GetPizzaById(string id)
        {
            return pizzas.FirstOrDefault(p => p.Id == id) ?? new Pizza { Id = "0", Nom = "Pizza non trouvée", Grandeur = "", Prix = 0.00m, Emotion = "" };
        }
    }
}
