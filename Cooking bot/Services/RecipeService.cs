using CookingBot.Models;
using MongoDB.Driver;

namespace CookingBot.Services
{
    public class RecipeService
    {
        private readonly IMongoCollection<Recipe> _recipes;

        public RecipeService(IMongoClient client)
        {
            var database = client.GetDatabase("CulinaryBotDB");
            _recipes = database.GetCollection<Recipe>("Recipes");
        }

        public async Task<List<Recipe>> GetByNameAsync(string name) =>
            await _recipes.Find(r => r.Title.ToLower().Contains(name.ToLower())).ToListAsync();

        public async Task<Recipe> GetByIdAsync(string id) =>
            await _recipes.Find(r => r.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Recipe recipe) =>
            await _recipes.InsertOneAsync(recipe);

        public async Task UpdateAsync(string id, Recipe recipe) =>
            await _recipes.ReplaceOneAsync(r => r.Id == id, recipe);

        public async Task DeleteAsync(string id) =>
            await _recipes.DeleteOneAsync(r => r.Id == id);
    }
}
