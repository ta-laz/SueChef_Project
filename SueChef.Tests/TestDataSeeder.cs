using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SueChef.Test
{
    using SueChef.Models;
    internal static class TestDataSeeder
    {
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
                    TRUNCATE TABLE "RecipeIngredients","Recipes","Ingredients","Chefs"
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
                    recipe.IsDairyFree  = dairyFree;
                    SetIfExists(recipe, "IsGlutenFree",  gf);
                    SetIfExists(recipe, "IsVegan",       vegan);
                    SetIfExists(recipe, "IsNutFree",     nutFree);
                    SetIfExists(recipe, "IsPescatarian", pesc);

                    db.Recipes.Update(recipe);
                    await db.SaveChangesAsync();
                }

                await tx.CommitAsync();
                Console.WriteLine("✅ Realistic recipes + ingredients seeded.");
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
                R("Margherita Pizza","Italian",2,
                  "Classic Neapolitan-style pizza with tomato, mozzarella, and basil.",
                  "Make or use a base; spread passata; add mozzarella; bake hot; finish with basil and olive oil.",
                  ("Passata",80,"g"), ("Mozzarella",90,"g"), ("Basil",2,"g"), ("Olive Oil",5,"ml"), ("Bread",120,"g") // using Bread as base proxy
                ),
                R("Chicken Tikka Masala","Indian",3,
                  "Grilled chicken in a creamy spiced tomato sauce.",
                  "Marinate chicken; grill; simmer sauce with tomato, cream and spices; combine and simmer.",
                  ("Chicken Breast",150,"g"), ("Greek Yoghurt",60,"g"), ("Passata",150,"g"), ("Onion",60,"g"),
                  ("Garlic",8,"g"), ("Garam Masala",4,"g"), ("Turmeric",2,"g"), ("Cumin",2,"g"), ("Double Cream",50,"g"),
                  ("Rice (white, dry)",75,"g")
                ),
                R("Greek Salad","Greek",1,
                  "Fresh salad with tomato, cucumber, feta, olives and oregano.",
                  "Chop veg; toss with olive oil and lemon; top with feta.",
                  ("Tomato",150,"g"), ("Cucumber",120,"g"), ("Onion",30,"g"), ("Feta",60,"g"),
                  ("Olive Oil",10,"ml"), ("Lemon",30,"g")
                ),
                R("Pad Thai (Prawn)","Thai",3,
                  "Stir-fried rice noodles with prawns, egg, and tamarind-like sauce (simplified).",
                  "Soak noodles; stir-fry prawns with aromatics; add egg; toss with sauce and noodles.",
                  ("Prawns",120,"g"), ("Eggs",1,null), ("Soy Sauce",15,"ml"), ("Oyster Sauce",10,"g"),
                  ("Vegetable Oil",10,"ml"), ("Lime",20,"g"), ("Rice (white, dry)",70,"g")
                ),
                R("Beef Bourguignon","French",3,
                  "Slow-cooked beef in red wine with mushrooms and onions.",
                  "Brown beef; sauté veg; simmer with stock and wine until tender.",
                  ("Beef Mince (10%)",200,"g"), ("Mushroom",80,"g"), ("Onion",80,"g"),
                  ("Carrot",60,"g"), ("Olive Oil",10,"ml"), ("Vegetable Stock",200,"ml")
                ),
                R("Shakshuka","Moroccan",1,
                  "Eggs poached in spiced tomato and pepper sauce.",
                  "Sauté peppers and onion; add garlic and spices; simmer tomato; crack in eggs and cook gently.",
                  ("Eggs",2,null), ("Tomato",200,"g"), ("Bell Pepper",100,"g"), ("Onion",60,"g"),
                  ("Garlic",10,"g"), ("Paprika",3,"g"), ("Cumin",2,"g"), ("Olive Oil",10,"ml")
                ),
                R("Pesto Pasta","Italian",1,
                  "Pasta tossed with basil pesto and parmesan.",
                  "Cook pasta; toss with pesto and a splash of cooking water; finish with parmesan.",
                  ("Pasta (dry)",90,"g"), ("Basil",10,"g"), ("Olive Oil",15,"ml"), ("Parmesan",20,"g"), ("Garlic",4,"g")
                ),
                R("Falafel Wrap","Lebanese",2,
                  "Falafel with salad and tahini-style dressing.",
                  "Mash chickpeas with herbs/spices; pan-fry; serve in wrap with veg.",
                  ("Chickpeas (canned)",120,"g"), ("Coriander",8,"g"), ("Parsley",8,"g"),
                  ("Cumin",3,"g"), ("Garlic",6,"g"), ("Olive Oil",10,"ml"), ("Tortilla (wheat)",60,"g"),
                  ("Lettuce",40,"g"), ("Tomato",60,"g")
                ),
                R("Salmon Teriyaki","Japanese",2,
                  "Salmon glazed with a soy-based teriyaki sauce.",
                  "Pan-sear salmon; reduce soy/sugar-like glaze; coat and serve with rice.",
                  ("Salmon Fillet",160,"g"), ("Soy Sauce",20,"ml"), ("Vegetable Oil",5,"ml"),
                  ("Rice (white, dry)",75,"g"), ("Spring Onion",30,"g") // use Onion as proxy if Spring Onion not in cat
                ),
                R("Miso Soup","Japanese",1,
                  "Light broth with tofu and spring onion (tofu proxied by Paneer for seeding).",
                  "Heat stock; dissolve miso (proxy); add tofu proxy and onion; simmer briefly.",
                  ("Vegetable Stock",300,"ml"), ("Onion",30,"g"), ("Paneer",40,"g")
                ),
                R("Tuna Poke Bowl","Hawaiian",2,
                  "Marinated raw tuna with rice and veg.",
                  "Cube tuna; marinate with soy; assemble over rice with veg.",
                  ("Tuna (raw)",120,"g"), ("Soy Sauce",15,"ml"), ("Rice (white, dry)",80,"g"),
                  ("Cucumber",80,"g"), ("Avocado",70,"g") // if missing Avocado, you can sub with "Olive Oil" 5ml + Cucumber
                ),
                R("Butter Chicken","Indian",3,
                  "Creamy tomato-butter chicken curry.",
                  "Marinate; grill; simmer in tomato, butter, cream; combine.",
                  ("Chicken Thigh",180,"g"), ("Greek Yoghurt",60,"g"), ("Passata",180,"g"),
                  ("Butter",20,"g"), ("Double Cream",40,"g"), ("Onion",60,"g"), ("Garlic",8,"g"),
                  ("Curry Powder",4,"g"), ("Rice (white, dry)",75,"g")
                ),
                R("Fish Tacos","Mexican",2,
                  "Pan-fried cod with slaw and lime in tortillas.",
                  "Season cod; pan-fry; assemble in tortillas with slaw; squeeze lime.",
                  ("Cod Fillet",150,"g"), ("Tortilla (wheat)",120,"g"), ("Cabbage",80,"g"),
                  ("Lime",25,"g"), ("Mayonnaise",20,"g")
                ),
                R("Veggie Stir-Fry","Chinese",2,
                  "Mixed vegetables stir-fried with soy and ginger.",
                  "Hot pan; oil; add veg in order; season with soy and ginger.",
                  ("Broccoli",100,"g"), ("Carrot",60,"g"), ("Bell Pepper",80,"g"), ("Mushroom",80,"g"),
                  ("Ginger",8,"g"), ("Garlic",6,"g"), ("Soy Sauce",20,"ml"), ("Vegetable Oil",10,"ml"),
                  ("Rice (white, dry)",70,"g")
                ),
                R("Chili Con Carne","American",2,
                  "Beef chili with beans and spices.",
                  "Brown beef; sauté aromatics; add tomato and spices; simmer; add beans.",
                  ("Beef Mince (10%)",180,"g"), ("Onion",70,"g"), ("Garlic",6,"g"), ("Passata",180,"g"),
                  ("Cumin",4,"g"), ("Paprika",4,"g"), ("Black Beans (cooked)",120,"g"),
                  ("Olive Oil",10,"ml"), ("Rice (white, dry)",75,"g")
                ),
                R("Lentil Dahl","Indian",1,
                  "Comforting spiced red lentils.",
                  "Toast spices; simmer lentils with onion, garlic, turmeric; finish with coriander.",
                  ("Lentils (dry)",70,"g"), ("Onion",60,"g"), ("Garlic",8,"g"), ("Turmeric",3,"g"),
                  ("Cumin",3,"g"), ("Olive Oil",10,"ml"), ("Rice (white, dry)",70,"g"), ("Coriander",6,"g")
                ),
                R("Hummus & Pita","Lebanese",1,
                  "Classic hummus served with bread.",
                  "Blend chickpeas with garlic, olive oil, lemon; serve with bread.",
                  ("Chickpeas (canned)",150,"g"), ("Garlic",5,"g"), ("Olive Oil",15,"ml"),
                  ("Lemon",30,"g"), ("Bread",100,"g")
                ),
                R("Tabbouleh","Lebanese",1,
                  "Herby salad with parsley, bulgur proxy (use Quinoa).",
                  "Cook quinoa; cool; combine with lots of parsley, tomato, lemon, oil.",
                  ("Quinoa (dry)",60,"g"), ("Parsley",25,"g"), ("Tomato",120,"g"),
                  ("Lemon",25,"g"), ("Olive Oil",10,"ml"), ("Mint",6,"g")
                ),
                R("Spaghetti Carbonara","Italian",2,
                  "Egg, parmesan, and cured pork sauce (pork shoulder proxy).",
                  "Cook pasta; toss with egg, cheese, and rendered pork off heat.",
                  ("Pasta (dry)",90,"g"), ("Eggs",1,null), ("Parmesan",25,"g"),
                  ("Pork Shoulder",60,"g"), ("Black Pepper",2,"g")
                ),
                R("Bolognese","Italian",2,
                  "Slow-simmered beef ragu with pasta.",
                  "Brown beef; add soffritto; tomato; simmer; serve over pasta.",
                  ("Beef Mince (10%)",180,"g"), ("Onion",60,"g"), ("Carrot",50,"g"), ("Celery",40,"g"),
                  ("Passata",200,"g"), ("Olive Oil",10,"ml"), ("Pasta (dry)",90,"g")
                ),
                R("Pho Ga","Vietnamese",2,
                  "Aromatic chicken noodle soup.",
                  "Simmer chicken in stock with ginger; add noodles; finish with herbs.",
                  ("Chicken Breast",140,"g"), ("Vegetable Stock",400,"ml"), ("Ginger",10,"g"),
                  ("Onion",40,"g"), ("Rice (white, dry)",70,"g"), ("Coriander",8,"g")
                ),
                R("Bibimbap","Korean",3,
                  "Warm rice bowl with mixed veg and beef (gochujang proxy via paprika+soy).",
                  "Cook rice; sauté toppings; assemble with egg and sauce.",
                  ("Rice (white, dry)",80,"g"), ("Beef Mince (10%)",120,"g"),
                  ("Spinach",70,"g"), ("Carrot",60,"g"), ("Mushroom",60,"g"),
                  ("Eggs",1,null), ("Soy Sauce",15,"ml"), ("Paprika",3,"g")
                ),
                R("Jerk Chicken","Caribbean",3,
                  "Spicy marinated chicken with rice.",
                  "Marinate chicken; grill/roast; serve with rice and lime.",
                  ("Chicken Thigh",200,"g"), ("Lime",20,"g"), ("Garlic",6,"g"),
                  ("Ginger",8,"g"), ("Vegetable Oil",10,"ml"), ("Rice (white, dry)",75,"g")
                ),
                R("Moroccan Chickpea Tagine","Moroccan",2,
                  "Spiced chickpeas with vegetables.",
                  "Sauté aromatics; add spices, chickpeas, veg; simmer.",
                  ("Chickpeas (canned)",160,"g"), ("Onion",60,"g"), ("Carrot",60,"g"),
                  ("Aubergine",80,"g"), ("Coriander",6,"g"), ("Cumin",3,"g"),
                  ("Paprika",3,"g"), ("Olive Oil",10,"ml")
                ),
                R("Seafood Paella (Prawn)","Spanish",3,
                  "Rice cooked with stock, saffron proxy, and prawns.",
                  "Sauté base; add rice and stock; simmer; add prawns near end.",
                  ("Prawns",140,"g"), ("Rice (white, dry)",90,"g"), ("Onion",60,"g"),
                  ("Bell Pepper",60,"g"), ("Vegetable Stock",350,"ml"), ("Olive Oil",10,"ml")
                ),
                R("Gazpacho","Spanish",1,
                  "Chilled tomato-cucumber soup.",
                  "Blend vegetables with oil and season; chill.",
                  ("Tomato",300,"g"), ("Cucumber",150,"g"), ("Onion",30,"g"),
                  ("Olive Oil",15,"ml"), ("Bread",40,"g")
                ),
                R("Ratatouille","French",2,
                  "Stewed Mediterranean vegetables.",
                  "Sauté each veg; combine and stew gently; finish with basil.",
                  ("Aubergine",120,"g"), ("Courgette",120,"g"), ("Bell Pepper",100,"g"),
                  ("Tomato",200,"g"), ("Onion",60,"g"), ("Garlic",8,"g"), ("Olive Oil",15,"ml"),
                  ("Basil",4,"g")
                ),
                R("Chicken Fajitas","Mexican",2,
                  "Spiced chicken with peppers and onions in tortillas.",
                  "Sear chicken; sauté peppers/onion; toss with spices; serve in tortillas.",
                  ("Chicken Breast",160,"g"), ("Bell Pepper",100,"g"), ("Onion",80,"g"),
                  ("Cumin",3,"g"), ("Paprika",3,"g"), ("Vegetable Oil",10,"ml"),
                  ("Tortilla (wheat)",120,"g")
                ),
                R("Tandoori Salmon","Indian",2,
                  "Yoghurt-spiced baked salmon.",
                  "Marinate salmon with yoghurt and spices; roast.",
                  ("Salmon Fillet",170,"g"), ("Greek Yoghurt",60,"g"),
                  ("Garam Masala",4,"g"), ("Turmeric",3,"g"), ("Lime",20,"g"),
                  ("Rice (white, dry)",75,"g")
                ),
                R("Moussaka","Greek",3,
                  "Layered aubergine with beef and béchamel (cream/butter proxy).",
                  "Pan-fry aubergine; cook beef in tomato; layer and bake with creamy top.",
                  ("Aubergine",200,"g"), ("Beef Mince (10%)",170,"g"), ("Onion",60,"g"),
                  ("Passata",180,"g"), ("Butter",20,"g"), ("Double Cream",60,"g")
                ),
                R("Chicken Souvlaki","Greek",2,
                  "Skewered marinated chicken with lemon and herbs.",
                  "Marinate; grill; serve with salad and bread.",
                  ("Chicken Breast",170,"g"), ("Lemon",25,"g"), ("Garlic",6,"g"),
                  ("Olive Oil",10,"ml"), ("Parsley",6,"g"), ("Bread",80,"g")
                ),
                R("Caprese Salad","Italian",1,
                  "Tomato, mozzarella, basil, olive oil.",
                  "Slice tomato and mozzarella; layer with basil; dress with oil.",
                  ("Tomato",200,"g"), ("Mozzarella",100,"g"), ("Basil",6,"g"), ("Olive Oil",10,"ml")
                ),
                R("Paneer Butter Masala","Indian",3,
                  "Creamy tomato curry with paneer.",
                  "Sauté aromatics and spices; add tomato and cream; simmer paneer.",
                  ("Paneer",150,"g"), ("Butter",20,"g"), ("Passata",180,"g"),
                  ("Onion",60,"g"), ("Garlic",8,"g"), ("Curry Powder",4,"g"),
                  ("Double Cream",40,"g"), ("Rice (white, dry)",75,"g")
                ),
                R("Chicken Katsu Curry","Japanese",3,
                  "Crispy chicken with mild curry sauce; rice.",
                  "Pan-fry chicken; make curry roux-like sauce; serve with rice.",
                  ("Chicken Breast",170,"g"), ("Vegetable Oil",10,"ml"),
                  ("Onion",60,"g"), ("Carrot",60,"g"), ("Curry Powder",5,"g"),
                  ("Vegetable Stock",250,"ml"), ("Rice (white, dry)",80,"g")
                ),
                R("Teriyaki Salmon Bowl","Japanese",2,
                  "Salmon teriyaki over rice with veg.",
                  "Sear salmon; glaze; serve over rice and veg.",
                  ("Salmon Fillet",160,"g"), ("Soy Sauce",20,"ml"), ("Rice (white, dry)",80,"g"),
                  ("Broccoli",100,"g"), ("Vegetable Oil",5,"ml")
                ),
                R("Chicken Caesar Salad","American",2,
                  "Romaine, chicken, parmesan, caesar dressing (approx).",
                  "Grill chicken; toss lettuce with dressing; shave parmesan.",
                  ("Chicken Breast",150,"g"), ("Lettuce",120,"g"), ("Parmesan",20,"g"),
                  ("Mayonnaise",20,"g"), ("Garlic",4,"g"), ("Bread",40,"g")
                ),
                R("Prawn Linguine","Italian",2,
                  "Garlic prawns tossed with pasta and lemon.",
                  "Sauté prawns with garlic/oil; toss with pasta and lemon juice.",
                  ("Prawns",140,"g"), ("Pasta (dry)",90,"g"), ("Garlic",8,"g"),
                  ("Olive Oil",12,"ml"), ("Lemon",25,"g"), ("Parsley",6,"g")
                ),
                R("Cottage Pie","British",3,
                  "Beef mince with mash topping (potato only here).",
                  "Cook beef with veg; top with mash; bake.",
                  ("Beef Mince (10%)",200,"g"), ("Onion",60,"g"), ("Carrot",60,"g"),
                  ("Vegetable Stock",200,"ml"), ("Olive Oil",10,"ml"), ("Potato",250,"g")
                ),
                R("Shepherd’s Pie","British",3,
                  "Lamb mince base with mash topping.",
                  "Cook lamb with veg and stock; top with mash; bake.",
                  ("Lamb Mince",200,"g"), ("Onion",60,"g"), ("Carrot",60,"g"),
                  ("Vegetable Stock",200,"ml"), ("Olive Oil",10,"ml"), ("Potato",250,"g")
                ),
                R("Chicken Noodle Soup","American",1,
                  "Light soup with chicken, vegetables and noodles.",
                  "Simmer chicken with veg; add pasta; cook until tender.",
                  ("Chicken Breast",130,"g"), ("Vegetable Stock",400,"ml"), ("Carrot",60,"g"),
                  ("Celery",40,"g"), ("Onion",40,"g"), ("Pasta (dry)",50,"g")
                ),
                R("Pancakes","American",1,
                  "Simple pancakes with butter (flour proxy via Bread).",
                  "Mix batter; pan-fry; serve with butter.",
                  ("Bread",80,"g"), ("Eggs",1,null), ("Milk",120,"ml"), ("Butter",10,"g")
                ),
                R("Cheese Omelette","French",1,
                  "Fluffy omelette with cheese.",
                  "Beat eggs; cook gently; add cheese; fold.",
                  ("Eggs",3,null), ("Butter",10,"g"), ("Cheddar",40,"g")
                ),
                R("Guacamole on Toast","Mexican",1,
                  "Mashed avocado with lime on toast (avocado proxy via olive oil + cucumber for texture).",
                  "Mash; season; spread on toast.",
                  ("Bread",80,"g"), ("Cucumber",60,"g"), ("Olive Oil",10,"ml"), ("Lime",20,"g")
                ),
                R("Bruschetta","Italian",1,
                  "Tomato, garlic, basil on toasted bread.",
                  "Chop tomato; season; spoon onto toast; finish with basil and oil.",
                  ("Bread",80,"g"), ("Tomato",150,"g"), ("Garlic",6,"g"),
                  ("Basil",6,"g"), ("Olive Oil",10,"ml")
                ),
                R("Chicken Burrito Bowl","Mexican",2,
                  "Spiced chicken with rice, beans, and veg.",
                  "Sear chicken with spices; serve over rice with beans and veg.",
                  ("Chicken Breast",160,"g"), ("Rice (white, dry)",80,"g"),
                  ("Black Beans (cooked)",120,"g"), ("Bell Pepper",80,"g"),
                  ("Onion",60,"g"), ("Cumin",3,"g"), ("Paprika",3,"g")
                ),
                R("Veggie Quesadilla","Mexican",1,
                  "Cheesy tortilla with sautéed vegetables.",
                  "Sauté veg; load tortilla with cheese; fold and griddle.",
                  ("Tortilla (wheat)",120,"g"), ("Cheddar",70,"g"),
                  ("Bell Pepper",60,"g"), ("Onion",40,"g"), ("Mushroom",60,"g")
                ),
                R("Spinach Ricotta Cannelloni","Italian",3,
                  "Pasta tubes filled with spinach & ricotta, baked in passata.",
                  "Mix filling; pipe into pasta proxy; bake with sauce.",
                  ("Pasta (dry)",90,"g"), ("Spinach",120,"g"), ("Ricotta",120,"g"),
                  ("Passata",200,"g"), ("Parmesan",20,"g"), ("Olive Oil",10,"ml")
                ),
                R("Minestrone Soup","Italian",1,
                  "Hearty vegetable soup with pasta and beans.",
                  "Sauté veg; add stock, tomato, pasta, beans; simmer.",
                  ("Vegetable Stock",400,"ml"), ("Passata",150,"g"), ("Onion",50,"g"),
                  ("Carrot",50,"g"), ("Celery",40,"g"), ("Courgette",70,"g"),
                  ("Pasta (dry)",50,"g"), ("Black Beans (cooked)",100,"g")
                ),
                R("Mushroom Risotto","Italian",3,
                  "Creamy risotto with mushrooms.",
                  "Toast rice; add stock gradually; finish with parmesan.",
                  ("Rice (white, dry)",90,"g"), ("Mushroom",120,"g"),
                  ("Vegetable Stock",500,"ml"), ("Onion",50,"g"),
                  ("Olive Oil",10,"ml"), ("Parmesan",25,"g")
                ),
                R("Halloumi Salad","Greek",1,
                  "Grilled halloumi with mixed salad.",
                  "Griddle halloumi; toss salad; dress with oil/lemon.",
                  ("Halloumi",100,"g"), ("Cucumber",100,"g"), ("Tomato",120,"g"),
                  ("Olive Oil",10,"ml"), ("Lemon",20,"g"), ("Lettuce",80,"g")
                ),
                R("Penne Arrabbiata","Italian",2,
                  "Spicy tomato pasta with garlic and chilli.",
                  "Cook pasta; simmer garlicky chilli tomato sauce; combine.",
                  ("Pasta (dry)",90,"g"), ("Passata",220,"g"), ("Garlic",8,"g"),
                  ("Chillies",4,"g"), ("Olive Oil",10,"ml"), ("Parsley",4,"g")
                ),
                R("Teriyaki Tofu Bowl (Paneer proxy)","Japanese",2,
                  "Sweet-savoury bowl with paneer acting as tofu for seeding.",
                  "Sear paneer; glaze; serve over rice with veg.",
                  ("Paneer",140,"g"), ("Soy Sauce",20,"ml"), ("Rice (white, dry)",80,"g"),
                  ("Broccoli",100,"g"), ("Vegetable Oil",5,"ml")
                ),
                R("Quinoa Buddha Bowl","American",2,
                  "Nutritious bowl with quinoa, veg, chickpeas.",
                  "Cook quinoa; roast/sauté veg; assemble with dressing.",
                  ("Quinoa (dry)",70,"g"), ("Chickpeas (canned)",120,"g"),
                  ("Spinach",80,"g"), ("Sweet Potato",150,"g"), ("Olive Oil",10,"ml")
                ),
                R("Prawn Fried Rice","Chinese",2,
                  "Quick fried rice with prawns and vegetables.",
                  "Use day-old rice; fry with eggs, prawns, veg; season.",
                  ("Rice (white, dry)",90,"g"), ("Prawns",130,"g"),
                  ("Eggs",1,null), ("Peas",80,"g"), ("Carrot",50,"g"), ("Soy Sauce",20,"ml"),
                  ("Vegetable Oil",10,"ml")
                ),
                R("Ramen (Simple)","Japanese",3,
                  "Noodle soup with chicken and egg.",
                  "Simmer stock; add noodles; top with chicken and egg.",
                  ("Vegetable Stock",500,"ml"), ("Chicken Breast",120,"g"),
                  ("Pasta (dry)",70,"g"), ("Eggs",1,null), ("Spring Onion",30,"g")
                ),
                R("Tuna Nicoise","French",2,
                  "Salad with tuna, egg, potato, green beans (proxy with broccoli).",
                  "Boil eggs and potato; assemble with tuna and veg; dress.",
                  ("Tuna (raw)",120,"g"), ("Eggs",1,null), ("Potato",180,"g"),
                  ("Tomato",120,"g"), ("Olive Oil",10,"ml"), ("Lemon",20,"g"), ("Broccoli",80,"g")
                ),
                R("Katsu Sando","Japanese",2,
                  "Crispy chicken sandwich.",
                  "Fry chicken; slice bread; assemble with mayo.",
                  ("Chicken Breast",150,"g"), ("Bread",100,"g"), ("Mayonnaise",20,"g"),
                  ("Vegetable Oil",10,"ml")
                ),
                R("Penne alla Vodka (no vodka)","Italian",2,
                  "Creamy tomato pasta (vodka omitted for test env).",
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
            var cats  = ingredients.Select(i => (i.Category ?? "").ToLowerInvariant()).ToHashSet();
            var names = ingredients.Select(i => (i.Name ?? "").ToLowerInvariant()).ToArray();

            bool hasMeat   = cats.Contains("meat") || cats.Contains("poultry");
            bool hasFish   = cats.Contains("fish") || cats.Contains("seafood");
            bool hasDairy  = cats.Contains("dairy"); // includes eggs by our catalogue grouping
            bool hasEggs   = names.Any(n => n.Contains("egg"));
            bool hasNuts   = cats.Contains("nuts");
            bool hasGluten = cats.Contains("pasta") || cats.Contains("bakery") || names.Any(n => n.Contains("wheat") || n.Contains("bread"));

            bool isVegetarian = !hasMeat && !hasFish;
            bool isVegan      = isVegetarian && !hasDairy && !hasEggs;
            bool isDairyFree  = !hasDairy || isVegan;
            bool isNutFree    = !hasNuts;
            bool isGlutenFree = !hasGluten;
            bool isPesc       = !hasMeat && hasFish;

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
