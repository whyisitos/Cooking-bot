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

        // GET: api/recipes/random
        [HttpGet("random")]
        public async Task<IActionResult> GetRandomRecipe()
        {
            var json = await _spoonacularService.GetRandomRecipeAsync();
            return Content(json, "application/json");
        }

        // GET: api/recipes/search?ingr=cheese,tomato
        [HttpGet("search")]
        public async Task<IActionResult> SearchByIngredients([FromQuery] string ingr)
        {
            var json = await _spoonacularService.SearchByIngredientsAsync(ingr);
            return Content(json, "application/json");
        }

        // GET: api/recipes/name/{name}
        [HttpGet("name/{name}")]
        public async Task<ActionResult<List<Recipe>>> GetByName(string name)
        {
            var localRecipes = await _recipeService.GetByNameAsync(name);
            var apiRecipes = await _spoonacularService.SearchByNameAsync(name);

            var combined = localRecipes.Concat(apiRecipes).ToList();
            return Ok(combined);
        }

        // POST: api/recipes
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] RecipeDto dto)
        {
            var recipe = new Recipe
            {
                Title = dto.Title,
                Ingredients = dto.Ingredients,
                Instructions = dto.Instructions
            };

            await _recipeService.CreateAsync(recipe);
            return CreatedAtAction(nameof(GetByName), new { name = recipe.Title }, recipe);
        }

        // PUT: api/recipes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] RecipeDto dto)
        {
            var existing = await _recipeService.GetByIdAsync(id);
            if (existing == null) return NotFound();

            var updated = new Recipe
            {
                Id = id,
                Title = dto.Title,
                Ingredients = dto.Ingredients,
                Instructions = dto.Instructions
            };

            await _recipeService.UpdateAsync(id, updated);
            return NoContent();
        }

        // DELETE: api/recipes/{id}
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
