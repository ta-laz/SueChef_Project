using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SueChef.Test
{
    using Microsoft.AspNetCore.Identity;
    using SueChef.Models;
    internal static class TestDataSeeder
    {

        private static readonly PasswordHasher<User> Hasher = new();
        private static readonly string PwHash = Hasher.HashPassword(new User(), "pass");
    
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
                // Clean slate
                await db.Database.ExecuteSqlRawAsync("""
                    TRUNCATE TABLE "RecipeIngredients","Recipes","Ingredients","Chefs",
                    "Ratings","MealPlanRecipes","MealPlans","Users"
                    RESTART IDENTITY CASCADE;
                """);

                // 1) Chefs (keep your names)
                var chefs = new List<Chef>
                {
                    new Chef { Name = "Karan Kullar" },
                    new Chef { Name = "Alex Lazar" },
                    new Chef { Name = "Jack Edwards" },
                    new Chef { Name = "Luisa Lanca" },
                    new Chef { Name = "Tom Wight" },
                    new Chef { Name = "Sarah Hunter" },
                    new Chef { Name = "Kiran Bhatt" },
                };
                db.AddRange(chefs);
                await db.SaveChangesAsync();

                // 2) Ingredient catalogue with realistic per-100g/ml macros
                var ings = SeedIngredientCatalogue();
                db.AddRange(ings);
                await db.SaveChangesAsync();

                // Index by name for quick lookup
                var ingByName = ings.ToDictionary(i => (i.Name ?? string.Empty).ToLowerInvariant());

                // 3) Curated recipes (50), Serving = 1
                var baseCreated = new DateTime(2025, 1, 1, 08, 00, 00, DateTimeKind.Utc);
                var recipes = BuildRecipes(); // definitions only (names, cuisine, steps, items)

                int chefIx = 0, picIx = 1, recNo = 0;
                foreach (var def in recipes)
                {
                    recNo++;
                    var chef = chefs[chefIx % chefs.Count]; chefIx++;

                    var (prep, cook) = EstimateTimes(def.DifficultyLevel, def.Items.Count, def.Cuisine, def.Title, recNo);

                    var recipe = new Recipe
                    {
                        Title = def.Title,
                        Description = def.Description,
                        Method = def.Method,
                        DifficultyLevel = def.DifficultyLevel,
                        IsVegetarian = false,                          // set after we analyse ingredients
                        IsDairyFree = false,                           // set after we analyse ingredients
                        Category = def.Cuisine,                        // cuisine in Category
                        ChefId = chef.Id,
                        CreatedAt = baseCreated.AddMinutes(recNo * 17),
                        RecipePicturePath = $"/images/recipes/{Slug(def.Title)}.jpg",
                        PrepTime = prep,
                        CookTime = cook
                    };

                    db.Recipes.Add(recipe);
                    await db.SaveChangesAsync(); // need Id

                    // Convert recipe items -> RecipeIngredients
                    var selectedIngredients = new List<Ingredient>();
                    foreach (var item in def.Items)
                    {
                        if (!ingByName.TryGetValue(item.Name.ToLowerInvariant(), out var ing))
                        {
                            throw new InvalidOperationException($"Ingredient '{item.Name}' not found in catalogue.");
                        }

                        selectedIngredients.Add(ing);

                        db.RecipeIngredients.Add(new RecipeIngredient
                        {
                            RecipeId = recipe.Id,
                            IngredientId = ing.Id,
                            Quantity = item.Quantity,
                            Unit = item.Unit // may be null (e.g. eggs)
                        });
                    }
                    await db.SaveChangesAsync();

                    // Compute diet flags from actual ingredients
                    var (veg, dairyFree, vegan, gf, nutFree, pesc) = ComputeDietaryFlags(selectedIngredients);
                    recipe.IsVegetarian = veg;
                    recipe.IsDairyFree = dairyFree;
                    SetIfExists(recipe, "IsGlutenFree", gf);
                    SetIfExists(recipe, "IsVegan", vegan);
                    SetIfExists(recipe, "IsNutFree", nutFree);
                    SetIfExists(recipe, "IsPescatarian", pesc);

                    db.Recipes.Update(recipe);
                    await db.SaveChangesAsync();
                }
                // Create 10 users
                var baseJoin = new DateOnly(2025, 1, 1);
                var baseDob = new DateOnly(1992, 1, 1);

                var users = Enumerable.Range(1, 10).Select(i => new User
                {
                    UserName = $"user{i}",
                    Email = $"user{i}@example.com",
                    PasswordHash = PwHash,
                    DateJoined = baseJoin.AddDays(i * 10),
                    DOB = baseDob.AddDays(i * 37)
                }).ToList();

                db.Users.AddRange(users);
                await db.SaveChangesAsync();

                // Meal Plans (0–2 per user)
                var mealPlans = new List<MealPlan>();
                var today = new DateOnly(2025, 10, 26);
                for (int i = 0; i < users.Count; i++)
                {
                    var u = users[i];
                    if (i % 3 == 0) continue; // some users have none
                    int planCount = (i % 3 == 1) ? 1 : 2;
                    for (int j = 0; j < planCount; j++)
                    {
                        var created = today.AddDays(-(i * 2 + j));
                        mealPlans.Add(new MealPlan
                        {
                            UserId = u.Id,
                            MealPlanTitle = $"{u.UserName} Plan {j + 1}",
                            CreatedOn = created,
                            UpdatedOn = created.AddDays(1)
                        });
                    }
                }
                db.MealPlans.AddRange(mealPlans);
                await db.SaveChangesAsync();

                // MealPlanRecipes (3–5 random recipes per plan)
                var totalRecipes = await db.Recipes.CountAsync();
                var planRecipes = new List<MealPlanRecipe>();
                var rand = new Random(42);

                foreach (var plan in mealPlans)
                {
                    int count = rand.Next(3, 6);
                    var picked = new HashSet<int>();
                    while (picked.Count < count)
                    {
                        int rid = rand.Next(1, totalRecipes + 1);
                        picked.Add(rid);
                    }
                    foreach (var rid in picked)
                    {
                        planRecipes.Add(new MealPlanRecipe
                        {
                            MealPlanId = plan.Id,
                            RecipeId = rid
                        });
                    }
                }
                db.MealPlanRecipes.AddRange(planRecipes);
                await db.SaveChangesAsync();

                // Ratings (each user rates 2–4 recipes)
                var ratings = new List<Rating>();
                int rateId = 1;
                foreach (var u in users)
                {
                    var picked = new HashSet<int>();
                    int rateCount = 2 + (u.Id % 3);
                    for (int j = 0; j < rateCount; j++)
                    {
                        int rid = ((u.Id * 7 + j * 11) % totalRecipes) + 1;
                        if (!picked.Add(rid)) continue;
                        ratings.Add(new Rating
                        {
                            RecipeId = rid,
                            UserId = u.Id,
                            Stars = 2 + ((u.Id + j) % 4),
                            CreatedOn = DateTime.UtcNow.AddDays(-u.Id * 3 - j)
                        });
                    }
                    rateId++;
                }
                db.Ratings.AddRange(ratings);
                await db.SaveChangesAsync();

                // Favorites (each user has 2–4)
                // var favorites = new List<Favorite>();
                // foreach (var u in users)
                // {
                //     var picked = new HashSet<int>();
                //     int favCount = 2 + (u.Id % 3);
                //     for (int j = 0; j < favCount; j++)
                //     {
                //         int rid = ((u.Id * 19 + j * 5) % totalRecipes) + 1;
                //         if (!picked.Add(rid)) continue;
                //         favorites.Add(new Favorite
                //         {
                //             UserId = u.Id,
                //             RecipeId = rid
                //         });
                //     }
                // }
                // db.Favorites.AddRange(favorites);
                // await db.SaveChangesAsync();

                await tx.CommitAsync();
                Console.WriteLine("✅ Database seeded.");
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

        // ---------- Ingredients (realistic per 100g/ml) ----------
        private static List<Ingredient> SeedIngredientCatalogue()
        {
            // NOTE: values are approximate reference-label macros per 100g (or 100ml for liquids).
            // You can refine later with your own data source if needed.
            var L = new List<Ingredient>
            {
                // Proteins / meats / fish / eggs
                I("Chicken Breast",       "meat",    165, 31.0f, 3.6f,  0.0f),
                I("Chicken Thigh",        "meat",    209, 26.0f, 10.9f, 0.0f),
                I("Beef Mince (10%)",     "meat",    217, 26.0f, 12.0f, 0.0f),
                I("Pork Shoulder",        "meat",    250, 24.0f, 17.0f, 0.0f),
                I("Lamb Mince",           "meat",    282, 25.0f, 20.0f, 0.0f),
                I("Salmon Fillet",        "fish",    208, 20.0f, 13.0f, 0.0f),
                I("Cod Fillet",           "fish",     82, 18.0f, 0.7f,  0.0f),
                I("Tuna (raw)",           "fish",    132, 29.0f, 1.0f,  0.0f),
                I("Prawns",               "seafood",  99, 24.0f, 0.3f,  0.0f),
                I("Eggs",                 "dairy",   155, 13.0f, 11.0f, 1.1f),

                // Dairy & alt
                I("Whole Milk",           "dairy",    61, 3.2f,  3.3f,  4.8f),
                I("Greek Yoghurt",        "dairy",    97, 9.0f,  5.2f,  3.6f),
                I("Cheddar",              "dairy",   403, 25.0f, 33.0f, 1.3f),
                I("Parmesan",             "dairy",   431, 38.0f, 29.0f, 4.1f),
                I("Feta",                 "dairy",   265, 14.0f, 21.0f, 4.1f),
                I("Butter",               "dairy",   717, 1.0f,  81.0f, 0.0f),
                I("Double Cream",         "dairy",   445, 2.0f,  48.0f, 2.9f),
                I("Plain Yoghurt",        "dairy",    59, 10.0f, 0.4f,  3.6f),

                // Oils & condiments
                I("Olive Oil",            "oils",    884, 0.0f,  100f,  0.0f),
                I("Vegetable Oil",        "oils",    884, 0.0f,  100f,  0.0f),
                I("Soy Sauce",            "condiments", 53, 8.0f, 0.6f, 5.6f),
                I("Tomato Paste",         "condiments", 82, 4.3f, 0.5f, 19.0f),
                I("Mayonnaise",           "condiments", 680, 1.0f, 75.0f, 1.0f),
                I("Fish Sauce",           "condiments", 50, 10.0f, 0.0f,  2.0f),
                I("Oyster Sauce",         "condiments", 51, 1.3f, 0.2f, 11.0f),
                I("Worcestershire Sauce", "condiments", 78, 0.0f,  0.0f, 19.0f),

                // Vegetables
                I("Onion",                "veg",      40, 1.1f,  0.1f,  9.3f),
                I("Garlic",               "veg",     149, 6.4f,  0.5f, 33.0f),
                I("Tomato",               "veg",      18, 0.9f,  0.2f,  3.9f),
                I("Bell Pepper",          "veg",      31, 1.0f,  0.3f,  6.0f),
                I("Carrot",               "veg",      41, 0.9f,  0.2f, 10.0f),
                I("Celery",               "veg",      16, 0.7f,  0.2f,  3.0f),
                I("Mushroom",             "veg",      22, 3.1f,  0.3f,  3.3f),
                I("Courgette",            "veg",      17, 1.2f,  0.3f,  3.1f),
                I("Aubergine",            "veg",      25, 1.0f,  0.2f,  6.0f),
                I("Spinach",              "veg",      23, 2.9f,  0.4f,  3.6f),
                I("Broccoli",             "veg",      34, 2.8f,  0.4f,  7.0f),
                I("Potato",               "veg",      77, 2.0f,  0.1f, 17.0f),
                I("Sweet Potato",         "veg",      86, 1.6f,  0.1f, 20.1f),
                I("Cucumber",             "veg",      16, 0.7f,  0.1f,  3.6f),
                I("Lettuce",              "veg",      15, 1.4f,  0.2f,  2.9f),
                I("Red Cabbage",          "veg",      31, 1.4f,  0.2f,  7.4f),
                I("Kale",                 "veg",      49, 4.3f,  0.9f,  8.8f),
                I("Ginger",               "spices",   80, 1.8f,  0.8f, 18.0f),
                I("Chillies",             "spices",   40, 2.0f,  0.4f,  9.0f),
                I("Spring Onion",         "veg",      40, 1.1f,  0.1f,  9.3f),
                I("Cabbage",          "veg",      31, 1.4f,  0.2f,  7.4f),
                I("Peas",              "veg",      23, 2.9f,  0.4f,  3.6f),

                // Fruit & herbs
                I("Lemon",                "fruit",    29, 1.1f,  0.3f,  9.3f),
                I("Lime",                 "fruit",    30, 0.7f,  0.2f, 10.5f),
                I("Mango",                "fruit",    60, 0.8f,  0.4f, 15.0f),
                I("Coriander",            "herbs",    23, 2.1f,  0.5f,  3.7f),
                I("Parsley",              "herbs",    36, 3.0f,  0.8f,  6.3f),
                I("Basil",                "herbs",    23, 3.2f,  0.6f,  2.7f),
                I("Mint",                 "herbs",    44, 3.8f,  0.7f,  8.0f),
                I("Dill",                 "herbs",    43, 3.5f,  1.1f,  7.0f),
                I("Avocado",              "fruit" ,    60, 0.8f,  0.4f, 15.0f),

                // Spices
                I("Cumin",                "spices",  375, 18.0f, 22.0f, 44.0f),
                I("Paprika",              "spices",  282, 14.0f, 13.0f, 54.0f),
                I("Turmeric",             "spices",  354, 8.0f,  10.0f, 65.0f),
                I("Curry Powder",         "spices",  325, 14.0f, 14.0f, 58.0f),
                I("Garam Masala",         "spices",  307, 15.0f, 13.0f, 53.0f),
                I("Black Pepper",              "spices",  282, 14.0f, 13.0f, 54.0f),

                // Legumes / pulses
                I("Chickpeas (canned)",   "legumes", 164, 9.0f,  2.6f, 27.0f),
                I("Lentils (dry)",        "legumes", 353, 25.0f, 1.1f,  60.0f),
                I("Black Beans (cooked)", "legumes", 132, 8.9f,  0.5f,  23.7f),

                // Grains / rice / pasta / bakery
                I("Rice (white, dry)",    "rice",    358, 6.7f,  0.9f,  79.0f),
                I("Rice (brown, dry)",    "rice",    362, 7.5f,  2.7f,  76.0f),
                I("Quinoa (dry)",         "grains",  368, 14.0f, 6.1f,  64.0f),
                I("Pasta (dry)",          "pasta",   371, 13.0f, 1.5f,  75.0f),
                I("Bread",                "bakery",  265, 9.0f,  3.2f,  49.0f),
                I("Tortilla (wheat)",     "bakery",  313, 8.1f,  8.5f,  49.0f),

                // Nuts / seeds
                I("Almonds",              "nuts",    579, 21.0f, 50.0f, 22.0f),
                I("Peanuts",              "nuts",    567, 26.0f, 49.0f, 16.0f),
                I("Sesame Seeds",         "seeds",   573, 18.0f, 50.0f, 23.0f),

                // Liquids / stocks
                I("Chicken Stock",        "condiments", 7, 0.5f, 0.2f, 0.5f),
                I("Vegetable Stock",      "condiments", 6, 0.3f, 0.1f, 0.8f),
                I("Coconut Milk",         "dairy",   230, 2.3f,  24.0f, 3.4f),
                I("Passata",              "veg",      30, 1.5f,  0.3f,  5.0f),
                I("Milk",                 "dairy",   230, 2.3f,  24.0f, 3.4f),

                // Cheeses & extras commonly used
                I("Mozzarella",           "dairy",   280, 28.0f, 17.0f, 3.1f),
                I("Ricotta",              "dairy",   174, 11.0f, 13.0f, 3.0f),
                I("Halloumi",             "dairy",   321, 22.0f, 26.0f, 2.2f),
                I("Paneer",               "dairy",   321, 21.0f, 25.0f, 3.6f),
            };

            // Clamp to max lengths (Name/Category up to 100 in your migration)
            foreach (var ing in L)
            {
                ing.Name = Truncate(ing.Name, 100);
                ing.Category = Truncate(ing.Category, 100);
            }

            return L;

            static Ingredient I(string name, string category, float kcal, float protein, float fat, float carbs)
                => new Ingredient { Name = name, Category = category, Calories = kcal, Protein = protein, Fat = fat, Carbs = carbs };
        }

        // ---------- Recipe definitions ----------
        private static List<RecipeDef> BuildRecipes()
        {
            // Short helper to keep items compact
            RecipeDef R(string title, string cuisine, int difficulty, string desc, string method, params (string Name, decimal Qty, string? Unit)[] items)
                => new(title, cuisine, difficulty, desc, method, items.ToList());

            // 50 realistic dishes, Serving = 1; quantities chosen accordingly.
            return new List<RecipeDef>
          {
              R("Classic Neapolitan Margherita Pizza","Italian",2,
                """
                A timeless pizzeria favourite with a thin, crisp base, rich tomato passata, and soft puddles of mozzarella.
                Fragrant basil and a final drizzle of olive oil bring fresh aromatics and balance to every slice.
                Ideal for quick Friday nights or a relaxed weekend bake.
                """,
                "Make or use a base; spread passata; add mozzarella; bake hot; finish with basil and olive oil.",
                ("Passata",80,"g"), ("Mozzarella",90,"g"), ("Basil",2,"g"), ("Olive Oil",5,"ml"), ("Bread",120,"g") // using Bread as base proxy
              ),
              R("Authentic Chicken Tikka Masala Curry","Indian",3,
                """
                Tender, yoghurt-marinated chicken, charred for smoky depth then simmered in a spiced tomato cream.
                Velvety, warming, and deeply savoury with notes of garam masala and turmeric throughout.
                Perfect with steamed rice and a scattering of fresh coriander.
                """,
                "Marinate chicken; grill; simmer sauce with tomato, cream and spices; combine and simmer.",
                ("Chicken Breast",150,"g"), ("Greek Yoghurt",60,"g"), ("Passata",150,"g"), ("Onion",60,"g"),
                ("Garlic",8,"g"), ("Garam Masala",4,"g"), ("Turmeric",2,"g"), ("Cumin",2,"g"), ("Double Cream",50,"g"),
                ("Rice (white, dry)",75,"g")
              ),
              R("Fresh Mediterranean Greek Salad Bowl","Greek",1,
                """
                Crunchy cucumber, juicy tomatoes, and sharp red onion tossed in lemon and olive oil.
                Creamy feta adds salty richness while herbs keep it bright and refreshing.
                A vibrant, no-cook classic that’s perfect for lunch or a light supper.
                """,
                "Chop veg; toss with olive oil and lemon; top with feta.",
                ("Tomato",150,"g"), ("Cucumber",120,"g"), ("Onion",30,"g"), ("Feta",60,"g"),
                ("Olive Oil",10,"ml"), ("Lemon",30,"g")
              ),
              R("Spicy Prawn Pad Thai Noodles","Thai",3,
                """
                A street-food favourite balancing sweet, salty, sour and heat.
                Juicy prawns, silky egg, and springy noodles tossed in a glossy, tangy soy-oyster glaze.
                Finish with lime for brightness and serve immediately while piping hot.
                """,
                "Soak noodles; stir-fry prawns with aromatics; add egg; toss with sauce and noodles.",
                ("Prawns",120,"g"), ("Eggs",1,null), ("Soy Sauce",15,"ml"), ("Oyster Sauce",10,"g"),
                ("Vegetable Oil",10,"ml"), ("Lime",20,"g"), ("Rice (white, dry)",70,"g")
              ),
              R("Slow-Cooked French Beef Bourguignon","French",3,
                """
                A rustic French stew brimming with tender beef, mushrooms, onions and carrots.
                Slowly simmered in stock and red wine for deep, layered flavour and silky richness.
                Comforting, elegant, and even better the next day.
                """,
                "Brown beef; sauté veg; simmer with stock and wine until tender.",
                ("Beef Mince (10%)",200,"g"), ("Mushroom",80,"g"), ("Onion",80,"g"),
                ("Carrot",60,"g"), ("Olive Oil",10,"ml"), ("Vegetable Stock",200,"ml")
              ),
              R("Moroccan Spiced Shakshuka Skillet","Moroccan",1,
                """
                Soft-poached eggs nestled in a bubbling sauce of tomatoes, peppers, and warm spices.
                Smoky paprika and cumin perfume the pan while the yolks stay rich and runny.
                Serve with crusty bread for scooping every last bite.
                """,
                "Sauté peppers and onion; add garlic and spices; simmer tomato; crack in eggs and cook gently.",
                ("Eggs",2,null), ("Tomato",200,"g"), ("Bell Pepper",100,"g"), ("Onion",60,"g"),
                ("Garlic",10,"g"), ("Paprika",3,"g"), ("Cumin",2,"g"), ("Olive Oil",10,"ml")
              ),
              R("Creamy Basil Pesto Pasta","Italian",1,
                """
                Al dente pasta swirled through a vivid basil pesto with parmesan and garlic.
                A silky emulsion of starchy pasta water and olive oil coats every strand.
                Simple ingredients, huge flavour — Italian comfort in minutes.
                """,
                "Cook pasta; toss with pesto and a splash of cooking water; finish with parmesan.",
                ("Pasta (dry)",90,"g"), ("Basil",10,"g"), ("Olive Oil",15,"ml"), ("Parmesan",20,"g"), ("Garlic",4,"g")
              ),
              R("Crispy Falafel Wrap With Tahini","Lebanese",2,
                """
                Herby chickpea patties pan-fried until golden and tucked into a soft wrap.
                Crunchy lettuce, juicy tomatoes, and creamy tahini bring freshness and richness.
                Great handheld fuel with classic Levantine flavours.
                """,
                "Mash chickpeas with herbs/spices; pan-fry; serve in wrap with veg.",
                ("Chickpeas (canned)",120,"g"), ("Coriander",8,"g"), ("Parsley",8,"g"),
                ("Cumin",3,"g"), ("Garlic",6,"g"), ("Olive Oil",10,"ml"), ("Tortilla (wheat)",60,"g"),
                ("Lettuce",40,"g"), ("Tomato",60,"g")
              ),
              R("Glazed Salmon Teriyaki Bowl","Japanese",2,
                """
                Succulent salmon lacquered in a sweet-savory soy glaze with subtle acidity.
                Served over fluffy rice and topped with spring onions for brightness and crunch.
                A speedy, satisfying weeknight bowl.
                """,
                "Pan-sear salmon; reduce soy/sugar-like glaze; coat and serve with rice.",
                ("Salmon Fillet",160,"g"), ("Soy Sauce",20,"ml"), ("Vegetable Oil",5,"ml"),
                ("Rice (white, dry)",75,"g"), ("Spring Onion",30,"g") // use Onion as proxy if Spring Onion not in cat
              ),
              R("Light Japanese Miso Soup","Japanese",1,
                """
                A soothing, umami-rich broth that’s gentle and restorative.
                Soft tofu and fresh spring onions add texture and aroma in every spoonful.
                Ideal as a starter or a calming lunch.
                """,
                "Heat stock; dissolve miso (proxy); add tofu proxy and onion; simmer briefly.",
                ("Vegetable Stock",300,"ml"), ("Onion",30,"g"), ("Paneer",40,"g")
              ),
              R("Fresh Tuna Poke Rice Bowl","Hawaiian",2,
                """
                Cubes of raw tuna marinated lightly in soy, paired with cool cucumber and ripe avocado.
                Served over warm rice for a bowl that balances clean flavours and satisfying textures.
                Bright, fresh, and endlessly craveable.
                """,
                "Cube tuna; marinate with soy; assemble over rice with veg.",
                ("Tuna (raw)",120,"g"), ("Soy Sauce",15,"ml"), ("Rice (white, dry)",80,"g"),
                ("Cucumber",80,"g"), ("Avocado",70,"g")
              ),
              R("Rich Butter Chicken Curry","Indian",3,
                """
                Charred, juicy chicken folded into a silky tomato-butter sauce enriched with cream.
                Gentle warmth from curry spices keeps it comforting without overpowering.
                A crowd-pleaser that’s luxurious yet familiar.
                """,
                "Marinate; grill; simmer in tomato, butter, cream; combine.",
                ("Chicken Thigh",180,"g"), ("Greek Yoghurt",60,"g"), ("Passata",180,"g"),
                ("Butter",20,"g"), ("Double Cream",40,"g"), ("Onion",60,"g"), ("Garlic",8,"g"),
                ("Curry Powder",4,"g"), ("Rice (white, dry)",75,"g")
              ),
              R("Crispy Cod Fish Tacos","Mexican",2,
                """
                Lightly seasoned cod tucked into soft tortillas with a crunchy slaw.
                A squeeze of lime and creamy mayo dressing bring brightness and richness.
                Perfect for taco night, any night.
                """,
                "Season cod; pan-fry; assemble in tortillas with slaw; squeeze lime.",
                ("Cod Fillet",150,"g"), ("Tortilla (wheat)",120,"g"), ("Cabbage",80,"g"),
                ("Lime",25,"g"), ("Mayonnaise",20,"g")
              ),
              R("Fast Veggie Soy Stir-Fry","Chinese",2,
                """
                A colourful medley of broccoli, peppers, mushrooms and carrot tossed hot and fast.
                Ginger and garlic perfume the wok while soy brings savoury depth.
                Serve over rice for a complete, speedy plate.
                """,
                "Hot pan; oil; add veg in order; season with soy and ginger.",
                ("Broccoli",100,"g"), ("Carrot",60,"g"), ("Bell Pepper",80,"g"), ("Mushroom",80,"g"),
                ("Ginger",8,"g"), ("Garlic",6,"g"), ("Soy Sauce",20,"ml"), ("Vegetable Oil",10,"ml"),
                ("Rice (white, dry)",70,"g")
              ),
              R("Hearty Chili Con Carne","American",2,
                """
                Savoury beef simmered with tomatoes, beans, and smoky spices until thick and rich.
                Comforting heat from cumin and paprika builds flavour without blowing your head off.
                Spoon over rice or pile into bowls with toppings.
                """,
                "Brown beef; sauté aromatics; add tomato and spices; simmer; add beans.",
                ("Beef Mince (10%)",180,"g"), ("Onion",70,"g"), ("Garlic",6,"g"), ("Passata",180,"g"),
                ("Cumin",4,"g"), ("Paprika",4,"g"), ("Black Beans (cooked)",120,"g"),
                ("Olive Oil",10,"ml"), ("Rice (white, dry)",75,"g")
              ),
              R("Comforting Red Lentil Dahl","Indian",1,
                """
                Creamy, spiced lentils gently scented with turmeric, cumin, garlic and onion.
                A wholesome bowl that’s nourishing, budget-friendly, and deeply satisfying.
                Serve with rice and fresh coriander.
                """,
                "Toast spices; simmer lentils with onion, garlic, turmeric; finish with coriander.",
                ("Lentils (dry)",70,"g"), ("Onion",60,"g"), ("Garlic",8,"g"), ("Turmeric",3,"g"),
                ("Cumin",3,"g"), ("Olive Oil",10,"ml"), ("Rice (white, dry)",70,"g"), ("Coriander",6,"g")
              ),
              R("Creamy Hummus With Pita","Lebanese",1,
                """
                Silky chickpea dip blended with garlic, lemon, and fruity olive oil.
                Spread generously and scoop with warm bread for the ultimate snack or starter.
                Simple ingredients, perfect texture.
                """,
                "Blend chickpeas with garlic, olive oil, lemon; serve with bread.",
                ("Chickpeas (canned)",150,"g"), ("Garlic",5,"g"), ("Olive Oil",15,"ml"),
                ("Lemon",30,"g"), ("Bread",100,"g")
              ),
              R("Herby Bulgur Tabbouleh Salad","Lebanese",1,
                """
                Bright, zesty salad packed with parsley, mint, tomatoes and lemon.
                Bulgur adds gentle bite while olive oil brings a rounded, fruity finish.
                A refreshing side that eats like a light meal.
                """,
                "Cook quinoa; cool; combine with lots of parsley, tomato, lemon, oil.",
                ("Quinoa (dry)",60,"g"), ("Parsley",25,"g"), ("Tomato",120,"g"),
                ("Lemon",25,"g"), ("Olive Oil",10,"ml"), ("Mint",6,"g")
              ),
              R("Classic Spaghetti Carbonara","Italian",2,
                """
                Glossy pasta cloaked in a silky emulsion of egg, parmesan and rendered pork.
                Rich, savoury and deceptively simple when timed just right.
                Peppery heat ties everything together.
                """,
                "Cook pasta; toss with egg, cheese, and rendered pork off heat.",
                ("Pasta (dry)",90,"g"), ("Eggs",1,null), ("Parmesan",25,"g"),
                ("Pork Shoulder",60,"g"), ("Black Pepper",2,"g")
              ),
              R("Slow-Simmered Beef Bolognese","Italian",2,
                """
                A classic ragu built on soffritto, beef, and tomato, simmered until thick and glossy.
                Deep, savoury flavours cling lovingly to each strand of pasta.
                Weeknight friendly, weekend worthy.
                """,
                "Brown beef; add soffritto; tomato; simmer; serve over pasta.",
                ("Beef Mince (10%)",180,"g"), ("Onion",60,"g"), ("Carrot",50,"g"), ("Celery",40,"g"),
                ("Passata",200,"g"), ("Olive Oil",10,"ml"), ("Pasta (dry)",90,"g")
              ),
              R("Aromatic Vietnamese Pho Ga","Vietnamese",2,
                """
                Fragrant chicken broth infused with ginger and onion, poured over tender meat and noodles.
                Fresh herbs on top add brightness and perfume in every slurp.
                Light yet wonderfully satisfying.
                """,
                "Simmer chicken in stock with ginger; add noodles; finish with herbs.",
                ("Chicken Breast",140,"g"), ("Vegetable Stock",400,"ml"), ("Ginger",10,"g"),
                ("Onion",40,"g"), ("Rice (white, dry)",70,"g"), ("Coriander",8,"g")
              ),
              R("Korean Bibimbap Rice Bowl","Korean",3,
                """
                A warm bowl layered with rice, seasoned vegetables, and savoury beef.
                Finished with a fried egg and a spicy, umami-rich sauce for mixing.
                Textural, colourful, and endlessly satisfying.
                """,
                "Cook rice; sauté toppings; assemble with egg and sauce.",
                ("Rice (white, dry)",80,"g"), ("Beef Mince (10%)",120,"g"),
                ("Spinach",70,"g"), ("Carrot",60,"g"), ("Mushroom",60,"g"),
                ("Eggs",1,null), ("Soy Sauce",15,"ml"), ("Paprika",3,"g")
              ),
              R("Fiery Caribbean Jerk Chicken","Caribbean",3,
                """
                Spicy, aromatic chicken marinated with ginger, garlic, lime and warm spices.
                Grilled or roasted until charred at the edges and juicy inside.
                Serve with rice to catch every drop of flavour.
                """,
                "Marinate chicken; grill/roast; serve with rice and lime.",
                ("Chicken Thigh",200,"g"), ("Lime",20,"g"), ("Garlic",6,"g"),
                ("Ginger",8,"g"), ("Vegetable Oil",10,"ml"), ("Rice (white, dry)",75,"g")
              ),
              R("Moroccan Chickpea Vegetable Tagine","Moroccan",2,
                """
                A gently spiced stew of chickpeas and seasonal vegetables.
                Paprika and cumin add warmth while coriander lifts the finish.
                Serve with couscous or flatbread for a wholesome meal.
                """,
                "Sauté aromatics; add spices, chickpeas, veg; simmer.",
                ("Chickpeas (canned)",160,"g"), ("Onion",60,"g"), ("Carrot",60,"g"),
                ("Aubergine",80,"g"), ("Coriander",6,"g"), ("Cumin",3,"g"),
                ("Paprika",3,"g"), ("Olive Oil",10,"ml")
              ),
              R("Spanish Seafood Paella With Prawns","Spanish",3,
                """
                Saffron-style rice cooked slowly with stock until plump and flavourful.
                Sweet peppers, onions, and juicy prawns make every forkful irresistible.
                A festive one-pan centrepiece.
                """,
                "Sauté base; add rice and stock; simmer; add prawns near end.",
                ("Prawns",140,"g"), ("Rice (white, dry)",90,"g"), ("Onion",60,"g"),
                ("Bell Pepper",60,"g"), ("Vegetable Stock",350,"ml"), ("Olive Oil",10,"ml")
              ),
              R("Chilled Andalusian Gazpacho Soup","Spanish",1,
                """
                A cool, refreshing blend of tomatoes, cucumber and onion with good olive oil.
                Light, zippy and perfect for hot days or a palate-cleansing starter.
                Best served well chilled.
                """,
                "Blend vegetables with oil and season; chill.",
                ("Tomato",300,"g"), ("Cucumber",150,"g"), ("Onion",30,"g"),
                ("Olive Oil",15,"ml"), ("Bread",40,"g")
              ),
              R("Provencal Vegetable Ratatouille","French",2,
                """
                A rustic stew of aubergine, courgette, peppers and tomatoes, cooked low and slow.
                Sweet, soft vegetables perfumed with garlic and basil.
                Delicious as a side or spooned over crusty bread.
                """,
                "Sauté each veg; combine and stew gently; finish with basil.",
                ("Aubergine",120,"g"), ("Courgette",120,"g"), ("Bell Pepper",100,"g"),
                ("Tomato",200,"g"), ("Onion",60,"g"), ("Garlic",8,"g"), ("Olive Oil",15,"ml"),
                ("Basil",4,"g")
              ),
              R("Sizzling Chicken Fajitas Feast","Mexican",2,
                """
                Strips of chicken tossed with peppers and onions, spiced and seared until smoky.
                Pile into warm tortillas with your favourite toppings.
                Weeknight-fast and always fun at the table.
                """,
                "Sear chicken; sauté peppers/onion; toss with spices; serve in tortillas.",
                ("Chicken Breast",160,"g"), ("Bell Pepper",100,"g"), ("Onion",80,"g"),
                ("Cumin",3,"g"), ("Paprika",3,"g"), ("Vegetable Oil",10,"ml"),
                ("Tortilla (wheat)",120,"g")
              ),
              R("Oven-Baked Tandoori Salmon","Indian",2,
                """
                Salmon fillets marinated in yoghurt and warm spices, roasted until flaky and tender.
                Tangy lime keeps the flavour bright while a side of rice completes the plate.
                Light, quick, and full of character.
                """,
                "Marinate salmon with yoghurt and spices; roast.",
                ("Salmon Fillet",170,"g"), ("Greek Yoghurt",60,"g"),
                ("Garam Masala",4,"g"), ("Turmeric",3,"g"), ("Lime",20,"g"),
                ("Rice (white, dry)",75,"g")
              ),
              R("Layered Greek Moussaka Bake","Greek",3,
                """
                Golden layers of aubergine and savoury beef beneath a creamy, enriched topping.
                Tomato and onion bring sweetness while butter and cream add indulgence.
                A hearty, crowd-pleasing casserole.
                """,
                "Pan-fry aubergine; cook beef in tomato; layer and bake with creamy top.",
                ("Aubergine",200,"g"), ("Beef Mince (10%)",170,"g"), ("Onion",60,"g"),
                ("Passata",180,"g"), ("Butter",20,"g"), ("Double Cream",60,"g")
              ),
              R("Lemon Herb Chicken Souvlaki","Greek",2,
                """
                Bright, garlicky chicken skewers marinated with lemon and herbs.
                Grilled for smoky edges and juicy centres, then served simply with salad and bread.
                Summer on a stick, any time of year.
                """,
                "Marinate; grill; serve with salad and bread.",
                ("Chicken Breast",170,"g"), ("Lemon",25,"g"), ("Garlic",6,"g"),
                ("Olive Oil",10,"ml"), ("Parsley",6,"g"), ("Bread",80,"g")
              ),
              R("Caprese Tomato Mozzarella Salad","Italian",1,
                """
                Thick-cut tomatoes and creamy mozzarella layered with fresh basil.
                A drizzle of olive oil ties together sweet, milky and herbaceous notes.
                Minimal ingredients, maximum flavour.
                """,
                "Slice tomato and mozzarella; layer with basil; dress with oil.",
                ("Tomato",200,"g"), ("Mozzarella",100,"g"), ("Basil",6,"g"), ("Olive Oil",10,"ml")
              ),
              R("Paneer Butter Masala Curry","Indian",3,
                """
                Cubes of paneer simmered in a gently spiced tomato-butter sauce.
                Cream adds a lush finish while aromatics build a comforting depth.
                A vegetarian favourite that feels totally luxurious.
                """,
                "Sauté aromatics and spices; add tomato and cream; simmer paneer.",
                ("Paneer",150,"g"), ("Butter",20,"g"), ("Passata",180,"g"),
                ("Onion",60,"g"), ("Garlic",8,"g"), ("Curry Powder",4,"g"),
                ("Double Cream",40,"g"), ("Rice (white, dry)",75,"g")
              ),
              R("Crispy Chicken Katsu Curry","Japanese",3,
                """
                Crunchy, golden chicken served with a mellow, aromatic curry sauce.
                Sweet carrot and onion round out the gravy; a bed of rice soaks up every drop.
                Comforting and irresistibly crisp.
                """,
                "Pan-fry chicken; make curry roux-like sauce; serve with rice.",
                ("Chicken Breast",170,"g"), ("Vegetable Oil",10,"ml"),
                ("Onion",60,"g"), ("Carrot",60,"g"), ("Curry Powder",5,"g"),
                ("Vegetable Stock",250,"ml"), ("Rice (white, dry)",80,"g")
              ),
              R("Teriyaki Salmon Rice Bowl","Japanese",2,
                """
                Flaky salmon glazed in a shiny teriyaki sauce and served over steamed rice.
                Broccoli adds freshness and bite, making the bowl balanced and complete.
                Quick to cook, big on flavour.
                """,
                "Sear salmon; glaze; serve over rice and veg.",
                ("Salmon Fillet",160,"g"), ("Soy Sauce",20,"ml"), ("Rice (white, dry)",80,"g"),
                ("Broccoli",100,"g"), ("Vegetable Oil",5,"ml")
              ),
              R("Classic Chicken Caesar Salad","American",2,
                """
                Charred chicken over crisp romaine with a creamy garlicky dressing.
                Parmesan brings savoury punch while garlicky croutons add crunch.
                A timeless staple with serious texture.
                """,
                "Grill chicken; toss lettuce with dressing; shave parmesan.",
                ("Chicken Breast",150,"g"), ("Lettuce",120,"g"), ("Parmesan",20,"g"),
                ("Mayonnaise",20,"g"), ("Garlic",4,"g"), ("Bread",40,"g")
              ),
              R("Garlic Prawn Lemon Linguine","Italian",2,
                """
                Sweet prawns sautéed with garlic and olive oil, tossed through silky pasta.
                A squeeze of lemon and fresh parsley keep things bright and aromatic.
                Elegant and fast — weeknight luxury.
                """,
                "Sauté prawns with garlic/oil; toss with pasta and lemon juice.",
                ("Prawns",140,"g"), ("Pasta (dry)",90,"g"), ("Garlic",8,"g"),
                ("Olive Oil",12,"ml"), ("Lemon",25,"g"), ("Parsley",6,"g")
              ),
              R("Traditional Cottage Pie Bake","British",3,
                """
                Savoury beef and vegetables simmered in stock then blanketed with buttery mash.
                Baked until golden with just-right crispy peaks on top.
                Proper comfort food for chilly evenings.
                """,
                "Cook beef with veg; top with mash; bake.",
                ("Beef Mince (10%)",200,"g"), ("Onion",60,"g"), ("Carrot",60,"g"),
                ("Vegetable Stock",200,"ml"), ("Olive Oil",10,"ml"), ("Potato",250,"g")
              ),
              R("Classic Shepherd’s Pie Bake","British",3,
                """
                Rich lamb mince base topped with creamy mashed potato and baked until bubbling.
                Sweet carrots and onion bring balance to the savoury filling.
                A timeless, family-style casserole.
                """,
                "Cook lamb with veg and stock; top with mash; bake.",
                ("Lamb Mince",200,"g"), ("Onion",60,"g"), ("Carrot",60,"g"),
                ("Vegetable Stock",200,"ml"), ("Olive Oil",10,"ml"), ("Potato",250,"g")
              ),
              R("Homestyle Chicken Noodle Soup","American",1,
                """
                A light, restorative broth with tender chicken, vegetables and soft noodles.
                Gentle flavours that soothe while still feeling nourishing.
                Ideal for cosy nights or under-the-weather days.
                """,
                "Simmer chicken with veg; add pasta; cook until tender.",
                ("Chicken Breast",130,"g"), ("Vegetable Stock",400,"ml"), ("Carrot",60,"g"),
                ("Celery",40,"g"), ("Onion",40,"g"), ("Pasta (dry)",50,"g")
              ),
              R("Fluffy Buttered Breakfast Pancakes","American",1,
                """
                Soft, golden pancakes with a tender crumb and buttery finish.
                Quick to whisk together and endlessly versatile for toppings.
                Weekend brunch, sorted.
                """,
                "Mix batter; pan-fry; serve with butter.",
                ("Bread",80,"g"), ("Eggs",1,null), ("Milk",120,"ml"), ("Butter",10,"g")
              ),
              R("Cheesy French Omelette","French",1,
                """
                A soft, custardy omelette folded around melting cheddar.
                Buttery, delicate and ready in minutes — a masterclass in simplicity.
                Add herbs if you fancy.
                """,
                "Beat eggs; cook gently; add cheese; fold.",
                ("Eggs",3,null), ("Butter",10,"g"), ("Cheddar",40,"g")
              ),
              R("Avocado Lime Toast Smash","Mexican",1,
                """
                Creamy avocado vibes with zesty lime on crunchy toast.
                A sunshine-bright snack that’s fast, fresh, and endlessly customisable.
                Breakfast, brunch, or any-time bite.
                """,
                "Mash; season; spread on toast.",
                ("Bread",80,"g"), ("Cucumber",60,"g"), ("Olive Oil",10,"ml"), ("Lime",20,"g")
              ),
              R("Tomato Basil Bruschetta Bites","Italian",1,
                """
                Toasted bread piled high with juicy tomatoes, garlic, and fresh basil.
                A drizzle of olive oil ties it all together for peak summer flavour.
                Ideal as an appetiser or light lunch.
                """,
                "Chop tomato; season; spoon onto toast; finish with basil and oil.",
                ("Bread",80,"g"), ("Tomato",150,"g"), ("Garlic",6,"g"),
                ("Basil",6,"g"), ("Olive Oil",10,"ml")
              ),
              R("Chicken Burrito Bowl Supper","Mexican",2,
                """
                Spiced chicken served over rice with beans, peppers and onions.
                Hearty, balanced and perfect for meal-prep or busy weeknights.
                Add your favourite toppings to finish.
                """,
                "Sear chicken with spices; serve over rice with beans and veg.",
                ("Chicken Breast",160,"g"), ("Rice (white, dry)",80,"g"),
                ("Black Beans (cooked)",120,"g"), ("Bell Pepper",80,"g"),
                ("Onion",60,"g"), ("Cumin",3,"g"), ("Paprika",3,"g")
              ),
              R("Cheesy Veggie Quesadilla","Mexican",1,
                """
                A golden, griddled tortilla stuffed with melty cheddar and sautéed vegetables.
                Crisp on the outside, oozy in the middle — pure comfort.
                Serve with salsa or a squeeze of lime.
                """,
                "Sauté veg; load tortilla with cheese; fold and griddle.",
                ("Tortilla (wheat)",120,"g"), ("Cheddar",70,"g"),
                ("Bell Pepper",60,"g"), ("Onion",40,"g"), ("Mushroom",60,"g")
              ),
              R("Spinach Ricotta Cannelloni Bake","Italian",3,
                """
                Tender pasta filled with creamy ricotta and wilted spinach, baked under rich tomato sauce.
                Finished with parmesan for a bronzed, savoury topping.
                A vegetarian classic that feels special.
                """,
                "Mix filling; pipe into pasta proxy; bake with sauce.",
                ("Pasta (dry)",90,"g"), ("Spinach",120,"g"), ("Ricotta",120,"g"),
                ("Passata",200,"g"), ("Parmesan",20,"g"), ("Olive Oil",10,"ml")
              ),
              R("Hearty Italian Minestrone Soup","Italian",1,
                """
                A chunky vegetable soup brimming with pasta, beans and tomatoes.
                Comforting and wholesome with plenty of texture in every spoonful.
                Perfect for batch-cooking.
                """,
                "Sauté veg; add stock, tomato, pasta, beans; simmer.",
                ("Vegetable Stock",400,"ml"), ("Passata",150,"g"), ("Onion",50,"g"),
                ("Carrot",50,"g"), ("Celery",40,"g"), ("Courgette",70,"g"),
                ("Pasta (dry)",50,"g"), ("Black Beans (cooked)",100,"g")
              ),
              R("Creamy Mushroom Risotto","Italian",3,
                """
                Arborio rice slowly cooked with stock until creamy and tender.
                Savoury mushrooms and parmesan bring umami depth in every bite.
                A hug in a bowl, Italian-style.
                """,
                "Toast rice; add stock gradually; finish with parmesan.",
                ("Rice (white, dry)",90,"g"), ("Mushroom",120,"g"),
                ("Vegetable Stock",500,"ml"), ("Onion",50,"g"),
                ("Olive Oil",10,"ml"), ("Parmesan",25,"g")
              ),
              R("Grilled Halloumi Garden Salad","Greek",1,
                """
                Squeaky-salty halloumi seared to golden and served with crisp greens and juicy tomatoes.
                Lemon and olive oil add bright acidity and fresh fruitiness.
                A satisfying, protein-rich salad.
                """,
                "Griddle halloumi; toss salad; dress with oil/lemon.",
                ("Halloumi",100,"g"), ("Cucumber",100,"g"), ("Tomato",120,"g"),
                ("Olive Oil",10,"ml"), ("Lemon",20,"g"), ("Lettuce",80,"g")
              ),
              R("Spicy Penne Arrabbiata","Italian",2,
                """
                A fiery tomato sauce scented with garlic and chilli, clinging to every ridge of penne.
                Bright, punchy and incredibly satisfying with a final gloss of olive oil.
                Midweek pasta with weekend swagger.
                """,
                "Cook pasta; simmer garlicky chilli tomato sauce; combine.",
                ("Pasta (dry)",90,"g"), ("Passata",220,"g"), ("Garlic",8,"g"),
                ("Chillies",4,"g"), ("Olive Oil",10,"ml"), ("Parsley",4,"g")
              ),
              R("Teriyaki Tofu Rice Bowl","Japanese",2,
                """
                Pan-seared tofu glazed in a sweet-savory sauce, served over warm rice and greens.
                A satisfying plant-forward bowl with great texture and balance.
                Add sesame seeds if you like.
                """,
                "Sear paneer; glaze; serve over rice with veg.",
                ("Paneer",140,"g"), ("Soy Sauce",20,"ml"), ("Rice (white, dry)",80,"g"),
                ("Broccoli",100,"g"), ("Vegetable Oil",5,"ml")
              ),
              R("Quinoa Power Buddha Bowl","American",2,
                """
                A nourishing bowl built on fluffy quinoa with chickpeas, spinach and sweet potato.
                Olive oil adds richness while the veg bring colour and crunch.
                Meal-prep friendly and deeply satisfying.
                """,
                "Cook quinoa; roast/sauté veg; assemble with dressing.",
                ("Quinoa (dry)",70,"g"), ("Chickpeas (canned)",120,"g"),
                ("Spinach",80,"g"), ("Sweet Potato",150,"g"), ("Olive Oil",10,"ml")
              ),
              R("Quick Prawn Fried Rice","Chinese",2,
                """
                Day-old rice fried hot with juicy prawns, peas and carrot.
                Egg adds richness while soy ties everything together with savoury depth.
                A proper takeaway classic at home.
                """,
                "Use day-old rice; fry with eggs, prawns, veg; season.",
                ("Rice (white, dry)",90,"g"), ("Prawns",130,"g"),
                ("Eggs",1,null), ("Peas",80,"g"), ("Carrot",50,"g"), ("Soy Sauce",20,"ml"),
                ("Vegetable Oil",10,"ml")
              ),
              R("Simple Chicken Ramen Bowl","Japanese",3,
                """
                A comforting noodle soup with tender chicken, rich broth and a soft egg.
                Spring onions add freshness while noodles make it hearty.
                Slurpworthy from first sip to last.
                """,
                "Simmer stock; add noodles; top with chicken and egg.",
                ("Vegetable Stock",500,"ml"), ("Chicken Breast",120,"g"),
                ("Pasta (dry)",70,"g"), ("Eggs",1,null), ("Spring Onion",30,"g")
              ),
              R("Classic Tuna Niçoise Salad","French",2,
                """
                A composed salad of tuna, egg, potatoes and green veg with a lemony dressing.
                Briny, bright and beautifully textural — a lunch that eats like a meal.
                Serve slightly warm or fully chilled.
                """,
                "Boil eggs and potato; assemble with tuna and veg; dress.",
                ("Tuna (raw)",120,"g"), ("Eggs",1,null), ("Potato",180,"g"),
                ("Tomato",120,"g"), ("Olive Oil",10,"ml"), ("Lemon",20,"g"), ("Broccoli",80,"g")
              ),
              R("Crispy Chicken Katsu Sando","Japanese",2,
                """
                A Japanese-inspired sandwich of crunchy chicken cutlet and soft bread.
                Creamy mayo brings richness while the crumb stays audibly crisp.
                A serious upgrade to lunch.
                """,
                "Fry chicken; slice bread; assemble with mayo.",
                ("Chicken Breast",150,"g"), ("Bread",100,"g"), ("Mayonnaise",20,"g"),
                ("Vegetable Oil",10,"ml")
              ),
              R("Creamy Penne Alla Vodka (Style)","Italian",2,
                """
                A blushing tomato-cream sauce that clings beautifully to penne.
                Silky, comforting and finished with parmesan for savoury depth.
                Weeknight-easy and restaurant-worthy.
                """,
                "Cook pasta; simmer passata with cream; combine with parmesan.",
                ("Pasta (dry)",90,"g"), ("Passata",220,"g"), ("Double Cream",60,"g"),
                ("Parmesan",25,"g"), ("Olive Oil",10,"ml")
              ),
          };

        }

        // ---------- Helpers ----------
        private static (bool veg, bool dairyFree, bool vegan, bool glutenFree, bool nutFree, bool pesc)
            ComputeDietaryFlags(IEnumerable<Ingredient> ingredients)
        {
            var cats = ingredients.Select(i => (i.Category ?? "").ToLowerInvariant()).ToHashSet();
            var names = ingredients.Select(i => (i.Name ?? "").ToLowerInvariant()).ToArray();

            bool hasMeat = cats.Contains("meat") || cats.Contains("poultry");
            bool hasFish = cats.Contains("fish") || cats.Contains("seafood");
            bool hasDairy = cats.Contains("dairy"); // includes eggs by our catalogue grouping
            bool hasEggs = names.Any(n => n.Contains("egg"));
            bool hasNuts = cats.Contains("nuts");
            bool hasGluten = cats.Contains("pasta") || cats.Contains("bakery") || names.Any(n => n.Contains("wheat") || n.Contains("bread"));

            bool isVegetarian = !hasMeat && !hasFish;
            bool isVegan = isVegetarian && !hasDairy && !hasEggs;
            bool isDairyFree = !hasDairy || isVegan;
            bool isNutFree = !hasNuts;
            bool isGlutenFree = !hasGluten;
            bool isPesc = !hasMeat && hasFish;

            return (isVegetarian, isDairyFree, isVegan, isGlutenFree, isNutFree, isPesc);
        }

        private static void SetIfExists(object obj, string propertyName, object? value)
        {
            var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (prop is { CanWrite: true })
            {
                if (prop.PropertyType == typeof(bool?) && value is bool b) prop.SetValue(obj, (bool?)b);
                else if (prop.PropertyType == typeof(bool) && value is bool b2) prop.SetValue(obj, b2);
                else prop.SetValue(obj, value);
            }
        }

        private static string Truncate(string? s, int len) => string.IsNullOrEmpty(s) ? string.Empty : (s.Length <= len ? s : s[..len]);

        private static string Slug(string title)
        {
            var s = new string((title ?? "").ToLowerInvariant()
                .Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray());
            while (s.Contains("--")) s = s.Replace("--", "-");
            return s.Trim('-');
        }

        // ---------- Simple record types for recipe definitions ----------
        private record RecipeDef(string Title, string Cuisine, int DifficultyLevel, string Description, string Method, List<(string Name, decimal Quantity, string? Unit)> Items);

        private static (int prep, int cook) EstimateTimes(int difficulty, int itemCount, string cuisine, string title, int index)
        {
            // base minutes by difficulty
            int basePrep = difficulty switch { 1 => 10, 2 => 15, _ => 20 };   // approx hands-on
            int baseCook = difficulty switch { 1 => 10, 2 => 20, _ => 35 };   // approx simmer/roast

            // scale a bit with ingredient count
            basePrep += Math.Clamp(itemCount - 5, -1, 5) * 2;  // +/- up to ~10 mins
            baseCook += Math.Clamp(itemCount - 5, -1, 5) * 3;  // +/- up to ~15 mins

            // cuisine nudges (rough heuristics)
            var c = cuisine.ToLowerInvariant();
            if (c is "italian") baseCook += 5;           // sauces / bakes
            else if (c is "indian" or "moroccan") baseCook += 10; // longer simmers
            else if (c is "japanese") basePrep += 5;     // slicing/plating
            else if (c is "mexican") basePrep += 3;

            // make it deterministic but varied per recipe index
            basePrep += (index % 3) * 2;
            baseCook += (index % 4) * 5;

            // clamp to sane ranges
            basePrep = Math.Clamp(basePrep, 5, 45);
            baseCook = Math.Clamp(baseCook, 5, 120);

            return (basePrep, baseCook);
        }

    }
}
