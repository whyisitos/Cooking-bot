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
        public async Task<Recipe> GetByNameSingleAsync(string name) =>
    await _recipes.Find(r => r.Title.ToLower() == name.ToLower()).FirstOrDefaultAsync();

        public async Task<Recipe> GetByIdAsync(string id) =>
            await _recipes.Find(r => r.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Recipe recipe) =>
            await _recipes.InsertOneAsync(recipe);

        public async Task UpdateAsync(string id, Recipe recipe) =>
            await _recipes.ReplaceOneAsync(r => r.Id == id, recipe);
        public async Task<bool> UpdateByNameAsync(string name, Recipe recipe)
        {
            var result = await _recipes.ReplaceOneAsync(r => r.Title.ToLower() == name.ToLower(), recipe);
            return result.ModifiedCount > 0;
        }

        public async Task DeleteAsync(string id) =>
            await _recipes.DeleteOneAsync(r => r.Id == id);
        public async Task<bool> DeleteByNameAsync(string name)
        {
            var result = await _recipes.DeleteOneAsync(r => r.Title.ToLower() == name.ToLower());
            return result.DeletedCount > 0;
        }
    }
}
