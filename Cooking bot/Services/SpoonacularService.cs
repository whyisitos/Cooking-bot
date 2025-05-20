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
            var url = $"https://api.spoonacular.com/recipes/complexSearch?query={name}&number=5&addRecipeInformation=true&apiKey={_apiKey}";
            var response = await _httpClient.GetStringAsync(url);

            var recipes = new List<Recipe>();
            var json = JObject.Parse(response);
            var results = json["results"];

            if (results != null)
            {
                foreach (var result in results)
                {
                    var title = result["title"]?.ToString() ?? "No title";
                    var instructions = result["instructions"]?.ToString() ?? "Instructions not available";
                    var ingredients = new List<string>();

                    var extendedIngredients = result["extendedIngredients"];
                    if (extendedIngredients != null)
                    {
                        foreach (var ing in extendedIngredients)
                        {
                            var nameClean = ing["nameClean"]?.ToString();
                            if (!string.IsNullOrWhiteSpace(nameClean))
                                ingredients.Add(nameClean);
                        }
                    }

                    recipes.Add(new Recipe
                    {
                        Title = title,
                        Instructions = instructions,
                        Ingredients = ingredients
                    });
                }
            }

            return recipes;
        }
    }
}

