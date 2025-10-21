using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SueChef.Tests
{
    // Adjust this using to your actual Models namespace
    using SueChef.Models;

    internal static class TestDataSeeder
    {
        // ---- Public entry points ------------------------------------------------

        public static async Task EnsureDbReadyAsync(SueChefDbContext db)
        {
            Console.WriteLine("🔧 Ensuring database is ready (migrate)...");
            await db.Database.MigrateAsync();
            Console.WriteLine("✅ Database schema ready.");
        }

        public static async Task ResetAndSeedAsync(SueChefDbContext db)
        {
            Console.WriteLine("🧹 Resetting and reseeding SueChef test database...");

            await db.Database.OpenConnectionAsync();
            await using var tx = await db.Database.BeginTransactionAsync();

            try
            {
                // 1) Clean slate (match table names from your migration)
                await db.Database.ExecuteSqlRawAsync("""
                    TRUNCATE TABLE "RecipeIngredients", "Recipes", "Ingredients", "Chefs"
                    RESTART IDENTITY CASCADE;
                """);

                // 2) Seed Chefs
                var chefs = SeedChefs();
                db.AddRange(chefs);
                await db.SaveChangesAsync();

                // 3) Seed Ingredients catalogue
                var ingredients = SeedIngredientCatalogue();
                db.AddRange(ingredients);
                await db.SaveChangesAsync();

                // 4) Seed Recipes (+ RecipeIngredients)
                var rng = new Random(1337); // deterministic for tests
                var cuisines = new[]
                {
                    "Mexican","Indian","Greek","Italian","Thai","Japanese","Chinese","French",
                    "Spanish","Turkish","Moroccan","Vietnamese","Korean","British","American",
                    "Ethiopian","Lebanese","Persian","Caribbean","Portuguese"
                };

                var baseCreated = new DateTime(2025, 1, 1, 8, 0, 0, DateTimeKind.Utc);

                var recipes = new List<Recipe>(50);
                var links   = new List<RecipeIngredient>(50 * 6);

                for (int i = 0; i < 50; i++)
                {
                    var chef = chefs[i % chefs.Count];
                    var cuisine = cuisines[i % cuisines.Length];

                    // Choose a small set of ingredients per recipe — biased by cuisine index for variety
                    var ingCount = 4 + (i % 4); // 4–7 ingredients
                    var picked = PickIngredients(ingredients, ingCount, rng, i);

                    var title = $"{cuisine} Dish {i + 1}";
                    var desc = $"A tasty {cuisine.ToLower()} recipe with {picked[0].Name.ToLower()}.";
                    
                    string[] methodSteps =
                    {
                        "Step 1. Prepare all ingredients by washing and chopping where necessary.",
                        "Step 2. Heat oil in a large pan over medium heat.",
                        "Step 3. Add aromatics such as garlic and onions; sauté until fragrant.",
                        "Step 4. Stir in the main ingredients and cook thoroughly.",
                        "Step 5. Add spices or sauces and simmer gently to blend flavours.",
                        "Step 6. Adjust seasoning and serve warm."
                    };

                    var method = string.Join("\n", methodSteps);

                    var serving = 2 + (i % 5);        // 2–6
                    var difficulty = 1 + (i % 3);     // 1–3 simple scale

                    // Compute dietary flags from ingredient categories
                    var (isVeg, isDairyFree, isVegan, isGlutenFree, isNutFree, isPesc) =
                        ComputeDietaryFlags(picked);

                    var recipe = new Recipe
                    {
                        Title = title,
                        Description = desc,
                        Serving = serving,
                        DifficultyLevel = difficulty,
                        IsVegetarian = isVeg,
                        IsDairyFree = isDairyFree,
                        Category = cuisine,
                        ChefId = chef.Id,
                        CreatedAt = baseCreated.AddMinutes(i * 17), // all different, deterministic
                        RecipePicturePath = $"/images/recipes/recipe-{i + 1}.jpg",
                        Method = method
                    };

                    // Optionally set properties that may not exist yet (reflection keeps this file compiling either way)
                    SetIfExists(recipe, "IsGlutenFree", isGlutenFree);
                    SetIfExists(recipe, "IsVegan",      isVegan);
                    SetIfExists(recipe, "IsNutFree",    isNutFree);
                    SetIfExists(recipe, "IsPescatarian",isPesc);

                    recipes.Add(recipe);
                    db.Recipes.Add(recipe);
                    await db.SaveChangesAsync(); // ensure Recipe.Id available for FK composite unique index

                    // Link ingredients with units/quantities
                    foreach (var (ing, slot) in picked.Select((x, idx) => (x, idx)))
                    {
                        var (qty, unit) = SuggestQuantityAndUnit(ing, rng, slot);
                        links.Add(new RecipeIngredient
                        {
                            RecipeId = recipe.Id,
                            IngredientId = ing.Id,
                            Quantity = qty,
                            Unit = unit  // may be null for unit items like eggs
                        });
                    }

                    // Ensure unique (RecipeId, IngredientId) for your unique index
                    var unique = links
                        .Where(li => li.RecipeId == recipe.Id)
                        .GroupBy(li => li.IngredientId)
                        .Select(g => g.First())
                        .ToList();

                    db.RecipeIngredients.AddRange(unique);
                    await db.SaveChangesAsync();
                }

                await tx.CommitAsync();
                Console.WriteLine("✅ Database reseeded successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Reseed failed: {ex.Message}");
                await tx.RollbackAsync();
                throw;
            }
            finally
            {
                await db.Database.CloseConnectionAsync();
            }
        }

        // ---- Helpers ------------------------------------------------------------

        private static List<Chef> SeedChefs() => new()
        {
            new Chef { Name = "Karan Kullar"  },
            new Chef { Name = "Alex Lazar"    },
            new Chef { Name = "Jack Edwards"  },
            new Chef { Name = "Luisa Lanca"   },
            new Chef { Name = "Tom Wight"     },
            new Chef { Name = "Sarah Hunter"  },
            new Chef { Name = "Kiran Bhatt"   },
        };

        private static List<Ingredient> SeedIngredientCatalogue()
        {
            // Name, Category, (Calories/Protein/Fat/Carbs) rough values per 100g or typical unit
            var list = new List<Ingredient>
            {
                // MEATS / POULTRY / FISH / SEAFOOD
                I("Chicken Breast", "poultry", 165, 31, 3.6f, 0),
                I("Beef Mince (10%)", "meat", 217, 26, 12, 0),
                I("Pork Shoulder", "meat", 250, 24, 17, 0),
                I("Lamb Leg", "meat", 206, 24, 12, 0),
                I("Salmon Fillet", "fish", 208, 20, 13, 0),
                I("Tuna", "fish", 132, 29, 1, 0),
                I("Prawns", "seafood", 99, 24, 0.3f, 0),
                I("Cod", "fish", 82, 18, 0.7f, 0),

                // DAIRY
                I("Whole Milk", "dairy", 61, 3.2f, 3.3f, 4.8f),
                I("Cheddar Cheese", "dairy", 403, 25, 33, 1.3f),
                I("Butter", "dairy", 717, 1, 81, 0),
                I("Yoghurt (Plain)", "dairy", 59, 10, 0.4f, 3.6f),
                I("Feta", "dairy", 265, 14, 21, 4.1f),

                // VEG
                I("Onion", "veg", 40, 1.1f, 0.1f, 9.3f),
                I("Garlic", "veg", 149, 6.4f, 0.5f, 33f),
                I("Tomato", "veg", 18, 0.9f, 0.2f, 3.9f),
                I("Bell Pepper", "veg", 31, 1, 0.3f, 6f),
                I("Spinach", "veg", 23, 2.9f, 0.4f, 3.6f),
                I("Courgette", "veg", 17, 1.2f, 0.3f, 3.1f),
                I("Aubergine", "veg", 25, 1, 0.2f, 6f),
                I("Mushroom", "veg", 22, 3.1f, 0.3f, 3.3f),
                I("Broccoli", "veg", 34, 2.8f, 0.4f, 7f),
                I("Carrot", "veg", 41, 0.9f, 0.2f, 10f),
                I("Potato", "veg", 77, 2, 0.1f, 17f),

                // FRUIT
                I("Lemon", "fruit", 29, 1.1f, 0.3f, 9.3f),
                I("Lime", "fruit", 30, 0.7f, 0.2f, 10.5f),
                I("Mango", "fruit", 60, 0.8f, 0.4f, 15f),

                // HERBS & SPICES
                I("Coriander", "herbs", 23, 2.1f, 0.5f, 3.7f),
                I("Parsley", "herbs", 36, 3, 0.8f, 6.3f),
                I("Basil", "herbs", 23, 3.2f, 0.6f, 2.7f),
                I("Cumin", "spices", 375, 18, 22, 44f),
                I("Paprika", "spices", 282, 14, 13, 54f),
                I("Chilli Powder", "spices", 282, 12, 15, 50f),
                I("Turmeric", "spices", 354, 8, 10, 65f),
                I("Ginger", "spices", 80, 1.8f, 0.8f, 18f),

                // LEGUMES
                I("Chickpeas (canned)", "legumes", 164, 9, 2.6f, 27f),
                I("Lentils (dry)", "legumes", 353, 25, 1.1f, 60f),
                I("Black Beans", "legumes", 339, 21, 1.2f, 62f),

                // GRAINS / PASTA / RICE / BAKERY
                I("Wheat Flour", "grains", 364, 10, 1, 76f),
                I("Pasta (wheat)", "pasta", 131, 5, 1.1f, 25f),
                I("Bread", "bakery", 265, 9, 3.2f, 49f),
                I("Rice (white)", "rice", 130, 2.7f, 0.3f, 28f),
                I("Rice (brown)", "rice", 111, 2.6f, 0.9f, 23f),
                I("Quinoa", "grains", 120, 4.4f, 1.9f, 21f),

                // NUTS / SEEDS / OILS / CONDIMENTS
                I("Almonds", "nuts", 579, 21, 50, 22f),
                I("Peanuts", "nuts", 567, 26, 49, 16f),
                I("Sesame Seeds", "seeds", 573, 18, 50, 23f),
                I("Olive Oil", "oils", 884, 0, 100, 0),
                I("Vegetable Oil", "oils", 884, 0, 100, 0),
                I("Soy Sauce", "condiments", 53, 8, 0.6f, 5.6f),
                I("Tomato Paste", "condiments", 82, 4.3f, 0.5f, 19f),

                // “Unit” style items (null unit acceptable)
                I("Eggs", "dairy", 155, 13, 11, 1.1f) // (biologically not dairy, but commonly grouped; used for dairy checks)
            };

            // Ensure max length constraints (Name: 100, Category: 100) from migration
            foreach (var ing in list)
            {
                ing.Name = Truncate(ing.Name, 100);
                ing.Category = Truncate(ing.Category, 100);
            }

            return list;

            static Ingredient I(string name, string cat, float kcal, float protein, float fat, float carb)
                => new Ingredient { Name = name, Category = cat, Calories = kcal, Protein = protein, Fat = fat, Carbs = carb };
        }

        private static List<Ingredient> PickIngredients(List<Ingredient> all, int count, Random rng, int recipeIndex)
        {
            // deterministic but “shuffled” selection
            var start = (recipeIndex * 7) % all.Count;
            var pool = all.Skip(start).Concat(all.Take(start)).ToList();

            // bias: ensure some base pattern — aromatics + oil + main + starch + veg
            var preferred = new List<string> { "veg", "spices", "herbs", "oils", "meat", "poultry", "fish", "seafood", "rice", "pasta", "grains", "bakery", "legumes", "dairy", "nuts" };

            var picked = new List<Ingredient>(count);
            foreach (var cat in preferred)
            {
                if (picked.Count >= count) break;
                var candidate = pool.FirstOrDefault(x => x.Category == cat);
                if (candidate != null && !picked.Any(p => p.Id == candidate.Id))
                    picked.Add(candidate);
            }

            // top-up if needed
            int cursor = 0;
            while (picked.Count < count && cursor < pool.Count)
            {
                var cand = pool[cursor++];
                if (!picked.Any(p => p.Name == cand.Name))
                    picked.Add(cand);
            }

            return picked.Take(count).ToList();
        }

        private static (decimal qty, string? unit) SuggestQuantityAndUnit(Ingredient ing, Random rng, int slot)
        {
            // Simple heuristics by category/name
            switch (ing.Category)
            {
                case "oils":
                    return (5 + (slot % 3) * 5, "ml"); // 5–15 ml
                case "condiments":
                    return (10 + (slot % 3) * 10, "g"); // 10–30 g
                case "spices":
                case "herbs":
                    return (2 + (slot % 2) * 2, "g");  // 2–4 g
                case "veg":
                case "fruit":
                    return (50 + (slot % 4) * 25, "g"); // 50–125 g
                case "legumes":
                case "grains":
                case "pasta":
                case "rice":
                case "bakery":
                    return (60 + (slot % 4) * 20, "g"); // 60–120 g
                case "meat":
                case "poultry":
                case "fish":
                case "seafood":
                    return (120 + (slot % 3) * 30, "g"); // 120–180 g
                case "nuts":
                case "seeds":
                    return (15 + (slot % 3) * 10, "g"); // 15–35 g
                case "dairy":
                    if (ing.Name.Contains("Milk", StringComparison.OrdinalIgnoreCase))
                        return (100 + (slot % 3) * 50, "ml"); // 100–200 ml
                    if (ing.Name.Contains("Yoghurt", StringComparison.OrdinalIgnoreCase))
                        return (50 + (slot % 3) * 25, "g"); // 50–100 g
                    if (ing.Name.Contains("Egg", StringComparison.OrdinalIgnoreCase))
                        return (2, null); // 2 eggs, unitless
                    return (20 + (slot % 3) * 20, "g"); // cheese/butter
                default:
                    return (25, "g");
            }
        }

        private static (bool veg, bool dairyFree, bool vegan, bool glutenFree, bool nutFree, bool pesc)
            ComputeDietaryFlags(IEnumerable<Ingredient> ingredients)
        {
            var cats = ingredients.Select(i => i.Category.ToLowerInvariant()).ToHashSet();
            var names = ingredients.Select(i => i.Name.ToLowerInvariant()).ToArray();

            bool hasMeat   = cats.Contains("meat") || cats.Contains("poultry");
            bool hasFish   = cats.Contains("fish") || cats.Contains("seafood");
            bool hasDairy  = cats.Contains("dairy"); // includes eggs here
            bool hasEggs   = names.Any(n => n.Contains("egg"));
            bool hasNuts   = cats.Contains("nuts");
            bool hasGlutenCategory = cats.Contains("grains") || cats.Contains("pasta") || cats.Contains("bakery");
            bool hasObviousGlutenName = names.Any(n =>
                n.Contains("wheat") || n.Contains("pasta") || n.Contains("bread"));

            bool isVegetarian = !hasMeat && !hasFish && !cats.Contains("seafood");
            bool isVegan      = isVegetarian && !hasDairy && !hasEggs;
            bool isDairyFree  = !hasDairy || isVegan;
            bool isNutFree    = !hasNuts;
            bool isGlutenFree = !(hasGlutenCategory || hasObviousGlutenName);
            bool isPesc       = !hasMeat && (hasFish || cats.Contains("seafood"));

            return (isVegetarian, isDairyFree, isVegan, isGlutenFree, isNutFree, isPesc);
        }

        private static void SetIfExists(object obj, string propertyName, object? value)
        {
            var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite)
            {
                // Coerce bool? for nullable properties if needed
                if (prop.PropertyType == typeof(bool?) && value is bool b)
                {
                    prop.SetValue(obj, (bool?)b);
                }
                else
                {
                    prop.SetValue(obj, value);
                }
            }
        }

        private static string Truncate(string s, int len) => s.Length <= len ? s : s[..len];
    }
}
