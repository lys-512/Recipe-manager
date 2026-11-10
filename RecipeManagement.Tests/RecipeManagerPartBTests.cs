using System.Collections.Generic;
using System.Linq;
using RecipeManagement.Core;

namespace RecipeManagement.Tests;

/// <summary>
/// Part B tests: LINQ title/ingredient search, protein report,
/// saved-recipe collection and one integrated workflow.
/// </summary>
public sealed class RecipeManagerPartBTests
{
    // ── B1 Title search ──

    [Fact]
    public void SearchByTitle_PartialMatch_IsCaseInsensitive()
    {
        var manager = CreateManager();

        var result = manager.SearchByTitle("curry");

        Assert.Single(result);
        Assert.Equal(101, result[0].Id);
        Assert.Single(manager.SearchByTitle("CUR"));
    }

    [Fact]
    public void SearchByTitle_Blank_ReturnsAllRecipes()
    {
        var manager = CreateManager();

        Assert.Equal(5, manager.SearchByTitle("").Count);
        Assert.Equal(5, manager.SearchByTitle("   ").Count);
    }

    [Fact]
    public void SearchByTitle_NoMatch_ReturnsEmpty()
    {
        var manager = CreateManager();

        Assert.Empty(manager.SearchByTitle("pizza"));
    }

    // ── B2 Ingredient search ──

    [Fact]
    public void SearchByIngredient_IsCaseInsensitive_AndReturnsEachRecipeOnce()
    {
        var manager = CreateManager();

        // Recipe 101 contains two ingredient strings that both match "rice".
        var result = manager.SearchByIngredient("RICE");

        Assert.Equal(new[] { 101, 105 }, result.Select(recipe => recipe.Id));
    }

    [Fact]
    public void SearchByIngredient_Blank_ReturnsAllRecipes()
    {
        var manager = CreateManager();

        Assert.Equal(5, manager.SearchByIngredient("").Count);
    }

    [Fact]
    public void SearchByIngredient_NoMatch_ReturnsEmpty()
    {
        var manager = CreateManager();

        Assert.Empty(manager.SearchByIngredient("chocolate"));
    }

    // ── B3 Protein report ──

    [Fact]
    public void HighestProteinReport_OrdersByProteinDescending()
    {
        var manager = CreateManager();

        var result = manager.GetHighestProteinRecipes(2);

        Assert.Equal(new[] { 101, 102 }, result.Select(recipe => recipe.Id));
        Assert.Equal(31.5, result[0].Nutrition!.ProteinG!.Value);
    }

    [Fact]
    public void HighestProteinReport_ExcludesRecipesWithoutProteinData()
    {
        var manager = CreateManager();

        // 104 has no nutrition object, 105 has nutrition but no protein value.
        var result = manager.GetHighestProteinRecipes(10);

        Assert.Equal(new[] { 101, 102, 103 }, result.Select(recipe => recipe.Id));
    }

    [Fact]
    public void HighestProteinReport_ZeroOrNegativeCount_ReturnsEmpty()
    {
        var manager = CreateManager();

        Assert.Empty(manager.GetHighestProteinRecipes(0));
        Assert.Empty(manager.GetHighestProteinRecipes(-3));
    }

    // ── B4 Saved recipes (HashSet<int>) ──

    [Fact]
    public void AddSavedRecipe_ValidId_ReturnsTrue()
    {
        var manager = CreateManager();

        Assert.True(manager.AddSavedRecipe(101));
        Assert.True(manager.IsRecipeSaved(101));
    }

    [Fact]
    public void AddSavedRecipe_Duplicate_ReturnsFalseAndKeepsOneEntry()
    {
        var manager = CreateManager();
        Assert.True(manager.AddSavedRecipe(102));

        Assert.False(manager.AddSavedRecipe(102));
        Assert.Single(manager.GetSavedRecipes());
    }

    [Fact]
    public void AddSavedRecipe_UnknownId_ReturnsFalse()
    {
        var manager = CreateManager();

        Assert.False(manager.AddSavedRecipe(999));
        Assert.False(manager.IsRecipeSaved(999));
    }

    [Fact]
    public void RemoveSavedRecipe_UpdatesMembership()
    {
        var manager = CreateManager();
        manager.AddSavedRecipe(103);

        Assert.True(manager.RemoveSavedRecipe(103));
        Assert.False(manager.IsRecipeSaved(103));
        Assert.False(manager.RemoveSavedRecipe(103));
    }

    [Fact]
    public void GetSavedRecipes_ReturnsOnlySavedIds()
    {
        var manager = CreateManager();
        manager.AddSavedRecipe(105);
        manager.AddSavedRecipe(101);

        Assert.Equal(new[] { 101, 105 }, manager.GetSavedRecipes());
    }

    // ── B5 Integrated Part B workflow ──

    [Fact]
    public void Integration_SearchReportAndSavedRecipesWorkWithPartACatalogue()
    {
        var manager = CreateManager();

        // Part B search on the existing Part A catalogue.
        var chickenRecipes = manager.SearchByTitle("chicken");
        Assert.Single(chickenRecipes);

        // Part B report on the same catalogue.
        var topProtein = manager.GetHighestProteinRecipes(1);
        Assert.Equal(101, topProtein[0].Id);

        // Save then remove one recipe, while Part A behaviour still works.
        Assert.True(manager.AddSavedRecipe(chickenRecipes[0].Id));
        Assert.True(manager.IsRecipeSaved(101));

        Assert.True(manager.AddRecipeToCookingPlan(101));
        Assert.True(manager.StartCooking(101));
        Assert.Equal("Brown the chicken", manager.CompleteNextInstruction());

        Assert.True(manager.RemoveSavedRecipe(101));
        Assert.False(manager.IsRecipeSaved(101));
        Assert.Equal(5, manager.RecipeCount);
    }

    private static RecipeManager CreateManager()
    {
        return new RecipeManager(new[]
        {
            new Recipe
            {
                Id = 101,
                Title = "Chicken Curry",
                Ingredients = new() { "2 chicken breasts", "1 cup rice", "1 cup rice noodles" },
                Instructions = new() { "Brown the chicken", "Simmer the curry" },
                Nutrition = new NutritionInfo { ProteinG = 31.5, CaloriesKcal = 420 }
            },
            new Recipe
            {
                Id = 102,
                Title = "Beef Stew",
                Ingredients = new() { "500g beef", "2 carrots" },
                Instructions = new() { "Sear the beef" },
                Nutrition = new NutritionInfo { ProteinG = 25.0 }
            },
            new Recipe
            {
                Id = 103,
                Title = "Vegetable Soup",
                Ingredients = new() { "1 onion", "2 carrots" },
                Instructions = new() { "Boil the vegetables" },
                Nutrition = new NutritionInfo { ProteinG = 4.0 }
            },
            new Recipe
            {
                Id = 104,
                Title = "Mystery Dish",
                Ingredients = new() { "unknown" },
                Instructions = new() { "Guess" },
                Nutrition = null
            },
            new Recipe
            {
                Id = 105,
                Title = "Rice Bowl",
                Ingredients = new() { "1 cup rice" },
                Instructions = new() { "Cook the rice" },
                Nutrition = new NutritionInfo { ProteinG = null }
            }
        });
    }
}
