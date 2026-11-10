# Recipe Management System — Parts A and B

Recipe manager built for the Software Development Practice assignment. The console menu
and the JSON loader are supplied by the course; `RecipeManager` in Core is implemented here.

## What is supplied

- `RecipeManagement.Core/Models/` — recipe model classes
- `RecipeManagement.Core/RecipeLoader.cs` — reads `data/recipes.json`
- `RecipeManagement.Core/IRecipeManager.cs` — public API
- `RecipeManagement.Application/` — console menu (options labelled PartA / PartB)
- `RecipeManagement.Tests/` — xUnit tests
- `data/recipes.json` — recipe dataset

## What is implemented

**Part A** — `RecipeManager.cs` using:

- `Dictionary<int, Recipe>` — recipe catalogue, lookup by ID
- `List<string>` — shopping list
- `LinkedList<int>` — cooking plan
- `Stack<int>` — removed-recipe history (LIFO restore)
- `Queue<string>` — cooking instruction queue (FIFO)

**Part B** — checks and report added on top of the Part A catalogue:

- `SearchByTitle` — case-insensitive partial title search (LINQ)
- `SearchByIngredient` — case-insensitive ingredient search (LINQ), each recipe returned once
- `GetHighestProteinRecipes` — the requested number of recipes with the highest protein value
- saved/favourite recipes — `HashSet<int>` with add, remove, membership check and listing
- `Design.md` — the collection choice, the integration and the basic complexity notes

## Requirements

- .NET SDK 10.0 or later (the projects target `net10.0`)
- No database or external service is needed; the dataset is the supplied `data/recipes.json`

## Build and test

Run these commands from the repository root (the folder containing `RecipeManagement.sln`):

```bash
dotnet restore
dotnet build
dotnet test
```

`dotnet test` runs the Part A tests and the Part B tests in `RecipeManagement.Tests`,
including the search, report, saved-recipe and integrated workflow cases.

## Run the application

```bash
dotnet run --project RecipeManagement.Application -- data/recipes.json
```

The menu options `15`–`21` exercise the Part B features:

- `15` search recipes by title
- `16` search recipes by ingredient
- `17` display the highest-protein recipes
- `18` save a recipe
- `19` remove a saved recipe
- `20` display saved recipes
- `21` check whether a recipe is saved

## Repository layout

```
RecipeManagement.sln
RecipeManagement.Application/   console menu and Program entry point
RecipeManagement.Core/          RecipeManager, loader, models, public API
RecipeManagement.Tests/         Part A tests and Part B tests
data/recipes.json               recipe dataset
Design.md                       Part B design explanation
```

## Submitted version

The submitted Part B version is the commit tagged `partB-submission` on the `main`
branch. `bin/` and `obj/` build output is excluded by `.gitignore`.

## AI acknowledgement

Replace this paragraph with the acknowledgement required by the assignment
specification. Use the required format and list the tool, the purpose, the topic
discussed, and how the submitted solution was developed independently. If no
generative AI tool was used, state that instead.
