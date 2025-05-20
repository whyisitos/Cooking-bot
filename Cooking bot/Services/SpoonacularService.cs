using CookingBot.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
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
            var url = $"https://api.spoonacular.com/recipes/random?number=1&apiKey={_apiKey}&addRecipeInformation=true&includeNutrition=true";
            return await _httpClient.GetStringAsync(url);
        }

        public async Task<List<Recipe>> SearchByNameAsync(string name)
        {
            var url = $"https://api.spoonacular.com/recipes/complexSearch?query={name}&number=5&addRecipeInformation=true&includeNutrition=true&apiKey={_apiKey}";
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
                    int calories = 0;

                    var nutrition = result["nutrition"]?["nutrients"];
                    if (nutrition != null)
                    {
                        foreach (var n in nutrition)
                        {
                            if (n["name"]?.ToString() == "Calories")
                            {
                                if (double.TryParse(n["amount"]?.ToString(), out var cal))
                                    calories = (int)System.Math.Round(cal);
                                break;
                            }
                        }
                    }

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
                        Ingredients = ingredients,
                        Calories = calories
                    });
                }
            }

            return recipes;
        }

        public async Task<List<Recipe>> SearchByIngredientsAsync(string ingredients)
        {
            var url = $"https://api.spoonacular.com/recipes/findByIngredients?ingredients={ingredients}&number=5&apiKey={_apiKey}";
            var response = await _httpClient.GetStringAsync(url);

            var parsed = JArray.Parse(response);
            var recipes = new List<Recipe>();

            foreach (var item in parsed)
            {
                var id = item["id"]?.ToString();
                var title = item["title"]?.ToString() ?? "No title";
                var usedIngredients = item["usedIngredients"]?.Select(i => i["name"]?.ToString()).Where(n => !string.IsNullOrWhiteSpace(n)) ?? [];
                var missedIngredients = item["missedIngredients"]?.Select(i => i["name"]?.ToString()).Where(n => !string.IsNullOrWhiteSpace(n)) ?? [];

                var allIngredients = usedIngredients.Concat(missedIngredients).Distinct().ToList();
                int calories = 0;
                string instructions = $"Інструкції: https://spoonacular.com/recipes/{title.Replace(' ', '-')}-{id}";

                if (!string.IsNullOrWhiteSpace(id))
                {
                    var nutritionUrl = $"https://api.spoonacular.com/recipes/{id}/nutritionWidget.json?apiKey={_apiKey}";
                    try
                    {
                        var nutritionResponse = await _httpClient.GetStringAsync(nutritionUrl);
                        var nutritionJson = JObject.Parse(nutritionResponse);
                        if (nutritionJson["calories"] != null)
                        {
                            var calString = nutritionJson["calories"].ToString().Split(' ').FirstOrDefault();
                            if (int.TryParse(calString, out var cal))
                                calories = cal;
                        }
                    }
                    catch
                    {
                    }
                }

                recipes.Add(new Recipe
                {
                    Title = title,
                    Instructions = instructions,
                    Ingredients = allIngredients,
                    Calories = calories
                });
            }

            return recipes;
        }
    }
}

