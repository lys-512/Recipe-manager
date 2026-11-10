# Design

Part B design notes: the saved-recipe collection, its integration with the Part A
system, and the expected complexity of the main operations.

## 1. Saved recipe collection

I selected **`HashSet<int>`** for the saved/favourite recipe feature. The set stores
only recipe IDs, because a saved recipe is always a reference to a recipe that already
exists in the Part A catalogue.

Why it suits the four required operations:

- **Add** — `Add(recipeId)` inserts the ID in average O(1). If the ID is already in the
  set, `Add` returns `false` and does not change the set, so duplicate prevention costs
  nothing extra and needs no separate search.
- **Remove** — `Remove(recipeId)` deletes the ID in average O(1) and returns `false`
  when the ID was not saved.
- **Membership check** — `Contains(recipeId)` hashes the ID and looks in one bucket,
  which is average O(1); no linear scan is needed.
- **Listing saved IDs** — `HashSet` has no indexer, so `GetSavedRecipes` copies the IDs
  into a `List<int>`, sorts them for a stable order and returns it as a read-only view.
  This is O(n) to copy.

Alternatives I rejected:

- `List<int>` — membership checks and duplicate prevention would each be a linear scan
  (O(n)), which is the operation this feature uses most.
- `Dictionary<int, bool>` — the value would never be read; a set expresses "membership
  only" directly and has the same average O(1) costs.

Only IDs that exist in the catalogue may be saved: `AddSavedRecipe` first checks
`FindRecipe(recipeId)`, so an unknown ID is rejected. What happens when a saved recipe
is later removed from the catalogue is outside the scope of this assignment.

## 2. Integration

The Part B features are added inside the existing `RecipeManager`, so they read the
same Part A catalogue (`Dictionary<int, Recipe> _recipes`) that the Part A methods
already use. Title search and ingredient search enumerate `_recipes.Values` and filter
each recipe with LINQ (`Where`, `Any`, `Contains` with `StringComparison.OrdinalIgnoreCase`);
the protein report filters on `Nutrition.ProteinG`, sorts with `OrderByDescending`
and `ThenBy`, and takes the requested number of rows with `Take`. The saved-recipe
feature keeps a separate `HashSet<int>` of catalogue IDs and validates every add
through `FindRecipe`. Because everything goes through `RecipeManager` and the supplied
`IRecipeManager` interface, the console menu, the JSON loader, the model classes and
all Part A behaviour stay unchanged, and the Part A tests continue to pass.

## 3. Basic complexity

| Operation | Structure | Expected complexity | Reason |
| --- | --- | --- | --- |
| Lookup recipe by ID | Dictionary | Average O(1) | Hash-based key lookup. |
| Traverse cooking plan | LinkedList | O(n) | Each planned recipe may need to be visited. |
| Complete next instruction | Queue | O(1) | The item at the front is removed. |
| LINQ title/ingredient search | Recipe collection | O(n) | Each recipe may need to be inspected. |
| Check whether a recipe is saved | HashSet<int> | Average O(1) | The ID is hashed and one bucket is checked; no scan of the saved IDs. |

Notes on the two Part B operations that are not in the table above:

- `GetHighestProteinRecipes` inspects every recipe once, so the filtering step is O(n);
  sorting the matches is O(k log k) where k is the number of recipes that have a protein
  value, and taking the first `count` items is O(count). In practice this is dominated by
  the sort, so the report is O(n log n) overall.
- `AddSavedRecipe` performs one average O(1) catalogue lookup and one average O(1) set
  insertion, so the whole operation is average O(1).
