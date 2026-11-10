using System.Collections.Generic;
using RecipeManagement.Core;

namespace RecipeManagement.Tests;

/// <summary>
/// Example tests from the assignment specification. Add your own tests as you work.
/// </summary>
public sealed class RecipeManagerTests
{
    [Fact]
    public void Constructor_BuildsRecipeDictionary()
    {
        var manager = CreateManager();
        Assert.Equal(2, manager.RecipeCount);
        Assert.Equal("Recipe A", manager.FindRecipe(10)?.Title);
    }

    [Fact]
    public void InstructionsAreCompletedInFileOrder()
    {
        var manager = CreateManager();
        Assert.True(manager.StartCooking(10));
        Assert.Equal("First step", manager.PeekNextInstruction());
        Assert.Equal("First step", manager.CompleteNextInstruction());
        Assert.Equal("Second step", manager.PeekNextInstruction());
    }

    [Fact]
    public void RemovedRecipesAreRestoredLastInFirstOut()
    {
        var manager = CreateManager();
        manager.AddRecipeToCookingPlan(10);
        manager.AddRecipeToCookingPlan(20);
        manager.RemoveRecipeFromCookingPlan(10);
        manager.RemoveRecipeFromCookingPlan(20);
        Assert.Equal(20, manager.PeekLastRemovedRecipe());
        Assert.True(manager.RestoreLastRemovedRecipe());
        Assert.Equal(new[] { 20 }, manager.GetCookingPlan());
    }

    // ── Dictionary：重复 ID / 缺失 ID / 非法输入 ──

    [Fact]
    public void AddRecipe_DuplicateId_ReturnsFalse()
    {
        var manager = CreateManager();
        var duplicate = new Recipe { Id = 10, Title = "Another A" };
        Assert.False(manager.AddRecipe(duplicate));
        Assert.Equal(2, manager.RecipeCount);
    }

    [Fact]
    public void FindRecipe_MissingId_ReturnsNull()
    {
        var manager = CreateManager();
        Assert.Null(manager.FindRecipe(999));
    }

    [Fact]
    public void AddRecipe_NonPositiveId_ReturnsFalse()
    {
        var manager = CreateManager();
        var bad = new Recipe { Id = 0, Title = "Zero ID" };
        Assert.False(manager.AddRecipe(bad));
    }

    [Fact]
    public void AddRecipe_BlankTitle_ReturnsFalse()
    {
        var manager = CreateManager();
        var bad = new Recipe { Id = 30, Title = "   " };
        Assert.False(manager.AddRecipe(bad));
    }

    // ── List：购物清单 ──

    [Fact]
    public void AddIngredientsToShoppingList_CopiesInOrder()
    {
        var manager = CreateManager();
        var added = manager.AddIngredientsToShoppingList(10);
        Assert.Equal(1, added);
        Assert.Equal(new[] { "1 apple" }, manager.GetShoppingList());
    }

    [Fact]
    public void AddIngredientsToShoppingList_MissingRecipe_ReturnsZero()
    {
        var manager = CreateManager();
        Assert.Equal(0, manager.AddIngredientsToShoppingList(999));
    }

    [Fact]
    public void ClearShoppingList_RemovesAllItems()
    {
        var manager = CreateManager();
        manager.AddIngredientsToShoppingList(10);
        Assert.Equal(1, manager.ShoppingItemCount);
        manager.ClearShoppingList();
        Assert.Equal(0, manager.ShoppingItemCount);
    }

    // ── LinkedList：烹饪计划去重和缺失 ──

    [Fact]
    public void AddRecipeToCookingPlan_Duplicate_ReturnsFalse()
    {
        var manager = CreateManager();
        Assert.True(manager.AddRecipeToCookingPlan(10));
        Assert.False(manager.AddRecipeToCookingPlan(10));
        Assert.Equal(1, manager.CookingPlanCount);
    }

    [Fact]
    public void AddRecipeToCookingPlan_MissingRecipe_ReturnsFalse()
    {
        var manager = CreateManager();
        Assert.False(manager.AddRecipeToCookingPlan(999));
    }

    // ── Stack：空栈处理 ──

    [Fact]
    public void PeekLastRemovedRecipe_EmptyStack_ReturnsNull()
    {
        var manager = CreateManager();
        Assert.Null(manager.PeekLastRemovedRecipe());
        Assert.Equal(0, manager.RemovedRecipeCount);
    }

    [Fact]
    public void RestoreLastRemovedRecipe_EmptyStack_ReturnsFalse()
    {
        var manager = CreateManager();
        Assert.False(manager.RestoreLastRemovedRecipe());
    }

    // ── Queue：空队列处理 ──

    [Fact]
    public void PeekNextInstruction_EmptyQueue_ReturnsNull()
    {
        var manager = CreateManager();
        Assert.Null(manager.PeekNextInstruction());
    }

    [Fact]
    public void CompleteNextInstruction_EmptyQueue_ReturnsNull()
    {
        var manager = CreateManager();
        Assert.Null(manager.CompleteNextInstruction());
        Assert.Equal(0, manager.PendingInstructionCount);
    }

    // ── 组件交互：端到端流程 ──

    [Fact]
    public void Integration_RecipeMovesThroughCataloguePlanAndCooking()
    {
        var manager = CreateManager();

        // 1. 食谱加入购物清单
        manager.AddIngredientsToShoppingList(10);
        Assert.Equal(1, manager.ShoppingItemCount);

        // 2. 食谱加入烹饪计划
        Assert.True(manager.AddRecipeToCookingPlan(10));
        Assert.Equal(new[] { 10 }, manager.GetCookingPlan());

        // 3. 从计划移除 → 进入历史栈
        Assert.True(manager.RemoveRecipeFromCookingPlan(10));
        Assert.Equal(0, manager.CookingPlanCount);
        Assert.Equal(1, manager.RemovedRecipeCount);

        // 4. 恢复 → 回到计划末尾
        Assert.True(manager.RestoreLastRemovedRecipe());
        Assert.Equal(new[] { 10 }, manager.GetCookingPlan());

        // 5. 开始烹饪 → 指令入队 → 逐条完成
        Assert.True(manager.StartCooking(10));
        Assert.Equal(2, manager.PendingInstructionCount);
        Assert.Equal("First step", manager.CompleteNextInstruction());
        Assert.Equal("Second step", manager.CompleteNextInstruction());
        Assert.Null(manager.CompleteNextInstruction());
    }

    private static RecipeManager CreateManager()
    {
        return new RecipeManager(new[]
        {
            new Recipe
            {
                Id = 10,
                Title = "Recipe A",
                Ingredients = new() { "1 apple" },
                Instructions = new() { "First step", "Second step" }
            },
            new Recipe
            {
                Id = 20,
                Title = "Recipe B"
            }
        });
    }
}
