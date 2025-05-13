namespace CookingBot.Models
{
    public class RecipeDto
    {
        public string Title { get; set; } = string.Empty;
        public List<string> Ingredients { get; set; } = new();
        public string Instructions { get; set; } = string.Empty;
    }
}
