using CookingBot.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CookingBot.Services
{
    public class SpoonacularService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public SpoonacularService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["SpoonacularApiKey"];
        }

        public async Task<string> GetRandomRecipeAsync()
        {
            var url = $"https://api.spoonacular.com/recipes/random?number=1&apiKey={_apiKey}";
            return await _httpClient.GetStringAsync(url);
        }

        public async Task<string> SearchByIngredientsAsync(string ingredients)
        {
            var url = $"https://api.spoonacular.com/recipes/findByIngredients?ingredients={ingredients}&number=5&apiKey={_apiKey}";
            return await _httpClient.GetStringAsync(url);
        }

        public async Task<List<Recipe>> SearchByNameAsync(string name)
        {
            var url = $"https://api.spoonacular.com/recipes/complexSearch?query={name}&number=5&apiKey={_apiKey}";
            var response = await _httpClient.GetStringAsync(url);

            var recipes = new List<Recipe>();
            var json = JObject.Parse(response);
            var results = json["results"];

            if (results != null)
            {
                foreach (var result in results)
                {
                    var id = result["id"]?.ToString();
                    if (string.IsNullOrWhiteSpace(id)) continue;

                    var detailsUrl = $"https://api.spoonacular.com/recipes/{id}/information?apiKey={_apiKey}";
                    var detailResponse = await _httpClient.GetStringAsync(detailsUrl);
                    var detailJson = JObject.Parse(detailResponse);

                    var title = detailJson["title"]?.ToString() ?? "No title";
                    var instructions = detailJson["instructions"]?.ToString() ?? "Instructions not available";
                    var ingredients = new List<string>();

                    var extendedIngredients = detailJson["extendedIngredients"];
                    if (extendedIngredients != null)
                    {
                        foreach (var ing in extendedIngredients)
                        {
                            var nameClean = ing["nameClean"]?.ToString();
                            if (!string.IsNullOrWhiteSpace(nameClean))
                                ingredients.Add(nameClean);
                        }
                    }

                    var recipe = new Recipe
                    {
                        Title = title,
                        Instructions = instructions,
                        Ingredients = ingredients,
                        Calories = detailJson["nutrition"]?["nutrients"]
                           ?.FirstOrDefault(n => n["name"]?.ToString() == "Calories")?["amount"]?.Value<double>() ?? 0
                    };

                    recipes.Add(recipe);
                }
            }

            return recipes;
        }
    }
}

