using CookingBot.Models;
using CookingBot.Services;
using Microsoft.AspNetCore.Mvc;

namespace CookingBot.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecipesController : ControllerBase
    {
        private readonly RecipeService _recipeService;
        private readonly SpoonacularService _spoonacularService;

        public RecipesController(RecipeService recipeService, SpoonacularService spoonacularService)
        {
            _recipeService = recipeService;
            _spoonacularService = spoonacularService;
        }

        [HttpGet("random")]
        public async Task<IActionResult> GetRandomRecipe()
        {
            var json = await _spoonacularService.GetRandomRecipeAsync();
            return Content(json, "application/json");
        }

        [HttpGet("search")]
        public async Task<ActionResult<List<Recipe>>> SearchByIngredients([FromQuery] string ingr)
        {
            var recipes = await _spoonacularService.SearchByIngredientsAsync(ingr);
            return Ok(recipes);
        }

        [HttpGet("name/{name}")]
        public async Task<ActionResult<List<Recipe>>> GetByName(string name)
        {
            var localRecipes = await _recipeService.GetByNameAsync(name);
            var apiRecipes = await _spoonacularService.SearchByNameAsync(name);

            var combined = localRecipes.Concat(apiRecipes).ToList();
            return Ok(combined);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Recipe recipe)
        {
            await _recipeService.CreateAsync(recipe);
            return CreatedAtAction(nameof(GetByName), new { name = recipe.Title }, recipe);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] Recipe recipe)
        {
            var existing = await _recipeService.GetByIdAsync(id);
            if (existing == null) return NotFound();

            recipe.Id = id;
            await _recipeService.UpdateAsync(id, recipe);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var existing = await _recipeService.GetByIdAsync(id);
            if (existing == null) return NotFound();

            await _recipeService.DeleteAsync(id);
            return NoContent();
        }
    }
}
