# 🍳 SueChef — Full-Stack MVC Meal Planner

> **"Your personalised meal planning companion — like BBC Good Food, but smarter."**

SueChef is a full-stack MVC web application developed as a Makers Academy final project. It allows users to browse recipes, plan weekly meals, generate shopping lists, and leave comments. The platform expands on familiar recipe applications by adding user interactivity, smart filtering, and persistent data storage.


## Quickstart

First, clone this repository. Then:

- Install the .NET Entity Framework CLI
  * `dotnet tool install --global dotnet-ef`
- Create the database/s in `psql`
  * `CREATE DATABASE suechef_development;`
  * `CREATE DATABASE suechef_test;`
- Run the migration to create the tables
  * `cd` into `/SueChef`
  * `dotnet ef database update`
  * `DATABASE_NAME=suechef_development dotnet ef database update`
- Start the application, with the development database
  * `DATABASE_NAME=suechef_development dotnet watch run`
- Go to `http://localhost:5287/`

## Running the Tests

- Install playwright package
  * `dotnet add package Microsoft.Playwright`
- Install playwright browser
  * `pwsh bin/Debug/net9.0/playwright.ps1 install`
- Verify its installation
 * `dotnet list package`
- Start the application, with the default (test) database
  * `dotnet watch run`
- Open a second terminal session and run the tests
  * `dotnet test`

### Troubleshooting

If you see a popup about not being able to open Chromedriver...
- Go to **System Preferences > Security and Privacy > General**
- There should be another message about Chromedriver there
- If so, Click on **Allow Anyway**

## Updating the Database

Changes are applied to the database programatically, using files called _migrations_, which live in the `/Migrations` directory. The process is as follows...

- To update an existing table
  * For example, you might want to add a title to the `Post` model
  * In which case, you would add a new field there
- To create a new table
  * For example, you might want to add a table called Comments
  * First, create the `Comment` model
  * Then go to Project3DbContext
  * And add this `public DbSet<Comment>? Comments { get; set; }` 
- Generate the migration file
  * `cd` into `/Project3`
  * Decide what you wan to call the migration file
  * `AddTitleToPosts` or `CreateCommentsTable` would be good descriptive names
  * Then do `dotnet ef migrations add ` followed by the name you chose
  * E.g.  `dotnet ef migrations add AddTitleToPosts`
- Run the migration
  * `dotnet ef database update`

### Troubleshooting

#### Seeing `role "postgres" does not exist`?

Your application tries to connect to the database as a user called `postgres`, which is normally created automatically when you install PostgresQL. If the `postgres` user doesn't exist, you'll see `role "postgres" does not exist`.

To fix it, you'll need to create the `postgres` user.

Try this in your terminal...

```
; createuser -s postgres
```

If you see `command not found: createuser`, start a new `psql` session and do this...

```sql
create user postgres;
```

#### Want to Change an Existing Migration?

Don't edit the migration files after they've been applied / run. If you do that, it'll probably lead to problems. If you decide that the migration you just applied wasn't quite right for some reason, you have two options

- Create and run another migration (using the process above)

OR...

- Rollback / undo the last migration
- Then edit the migration file before re-running it

How do you rollbacl a migration? Let's assume that you have two migrations, both of which have been applied.

1. CreatePostsAndUsers
2. AddTitleToPosts

To rollback the second, you again use `dotnet ef database update` but this time adding the name of the last 'good' migration. In this case, that would be `CreatePostsAndUsers`. So the command is...

```shell
; dotnet ef database update CreatePostsAndUsers
```

---

## 🚀 Overview

**Local URL:** [`http://localhost:5179`](http://localhost:5179)

**Tech Stack**
| Layer | Technology |
|-------|-------------|
| Backend | ASP.NET Core MVC (C#) |
| Database | PostgreSQL — `suechef_development`, `suechef_test` |
| ORM | Entity Framework Core |
| Frontend | Razor Views, Tailwind CSS, JavaScript |
| Testing | Playwright (UI), xUnit (Unit) |
| Auth | Custom session-based authentication filter |

---

## 📁 Project Structure

| Folder | Description |
|--------|--------------|
| `/Controllers` | Request handling and route logic |
| `/Models` | Database entities and EF Core schemas |
| `/Views` | Razor pages and UI rendering |
| `/ViewModels` | Data transfer and view binding models |
| `/wwwroot` | Static files (CSS, JS, images) |
| `/Data` | EF Core DbContext and migrations |
| `/ActionFilters` | Session authentication filter |

---
### Request Flow

```
[Browser] → [Controller] → [Model/DbContext] → [Controller] → [View] → [Browser]
```
### Application Layers

| Layer | Purpose | Example Files |
|--------|----------|----------------|
| **Model** | Defines schema, validation, and logic | `UserModel.cs`, `RecipeModel.cs`, `MealPlanModel.cs` |
| **View** | Renders Razor pages with Tailwind UI | `_Layout.cshtml`, `RecipeDetails/Index.cshtml` |
| **Controller** | Routes and handles logic | `MealPlanController.cs`, `RecipeDetailsController.cs` |
| **Database Context** | EF Core ORM bridge | `SueChefDbContext.cs` |

### Design Principles

- **Entity Framework Core** for ORM and migrations  
- **Tailwind CSS** for clean, responsive UI  
- **Session-based authentication** for simplicity and security  
- **Custom Action Filter** for route protection (`AuthenticationFilter.cs`)  
- **Reusable ViewModels** for componentised front-end rendering  

---

## 🗄️ Database Schema & Migrations

| Model | Purpose |
|--------|----------|
| **Recipe** | Stores recipe metadata (title, ingredients, cooking time, difficulty, chef). |
| **MealPlan** | Represents a user's meal plan, linking multiple recipes by day. |
| **Ingredient** | Ingredient name, unit, and relationship to recipes. |
| **RecipeIngredient** | Join table for many-to-many between Recipe and Ingredient. |
| **MealPlanRecipes** | Associates MealPlans with selected Recipes. |
| **Comment** | Stores user-generated comments per recipe. |
| **User** | Authenticated users (credentials, preferences). |
| **Rating** | Star ratings for recipes. |
| **Chef** | Author attribution for each recipe. |
| **Favourites** | Soft-deletable user-to-recipe favourites list. |
| **ShoppingList** | Tracks items generated from meal plans. |

### 3.3 Relationships

- **User ↔ Session/Auth**
  - Users sign up, sign in, and maintain a server-side session (custom authentication filter).
- **MealPlan *—* Recipe (via MealPlanRecipes)**
  - A meal plan contains many recipes; a recipe can appear in many meal plans (join table `MealPlanRecipes`, typically with day/position metadata).
- **Recipe *—* Ingredient (via RecipeIngredient)**
  - A recipe uses many ingredients; an ingredient can be used by many recipes (join table `RecipeIngredient` with quantity/unit).
- **Recipe —* Comment / User —* Comment**
  - A recipe has many comments; a user can write many comments. Each comment belongs to exactly one user and one recipe.
- **Recipe —* Rating / User —* Rating**
  - A recipe can have many ratings; a user can submit (at most) one rating per recipe (enforced in code or DB unique constraint). Each rating links a user to a recipe with a score.
- **Recipe *— Chef**
  - Each recipe is authored by a single chef; a chef can author many recipes.
- **User *—* Recipe (via Favourites)**
  - Users can save many favourite recipes; recipes can be favourited by many users. `Favourites` rows may be **soft-deleted** (e.g., `IsDeleted` flag) to support undo/restore.
- **User —* ShoppingList**
  - A user can maintain a shopping list consisting of multiple line items (ingredient, quantity, unit, purchased flag). Shopping list lines are independent records tied to the user; they’re often generated from one or more meal plans.
- **Categories (logical)**
  - Categories are used for filtering and display (carousels, index). Recipes are associated with one category (or a small set) depending on your model; filtering in the `SearchBarController` projects category/chef/ingredient/difficulty/duration facets into the `SearchPageViewModel`.

**Key business rules**
- Difficulty is stored numerically and rendered via a switch to **Easy/Medium/Hard**.
- Favourite toggling updates or soft-deletes an existing row rather than creating duplicates.
- Search combines free text (`searchQuery`) with structured filters (category, ingredients [multi-select], chef, difficulty, duration buckets, and dietary flags) to produce a set of `RecipeCardViewModel`s.
- Comments and ratings require an authenticated session (guarded by the authentication filter).

---

### 3.4 ER Diagram

```mermaid
erDiagram
  USER ||--o{ MEALPLAN : "owns"
  USER ||--o{ COMMENT : "writes"
  USER ||--o{ RATING : "rates"
  USER ||--o{ FAVOURITES : "saves"
  USER ||--o{ SHOPPINGLIST : "has items"

  CHEF ||--o{ RECIPE : "authors"

  RECIPE ||--o{ COMMENT : "receives"
  RECIPE ||--o{ RATING : "receives"
  RECIPE ||--o{ RECIPEINGREDIENT : "includes"
  RECIPE ||--o{ MEALPLANRECIPES : "appears in"

  INGREDIENT ||--o{ RECIPEINGREDIENT : "used in"

  MEALPLAN ||--o{ MEALPLANRECIPES : "contains"
  
  %% Junctions / Associatives
  RECIPEINGREDIENT {
    int RecipeId
    int IngredientId
    decimal Quantity
    string Unit
  }

  MEALPLANRECIPES {
    int MealPlanId
    int RecipeId
    string DayOrSlot  // optional if present
  }

  %% Core entities
  USER {
    int Id
    string Username
    string Email
    string PasswordHash
  }

  CHEF {
    int Id
    string Name
  }

  RECIPE {
    int Id
    string Title
    int TotalMinutes
    int Difficulty  // 1=Easy,2=Medium,3=Hard
    int ChefId
    string Category  // if modelled as a scalar field
  }

  INGREDIENT {
    int Id
    string Name
  }

  COMMENT {
    int Id
    int UserId
    int RecipeId
    datetime CreatedOn
    string Content
  }

  RATING {
    int Id
    int UserId
    int RecipeId
    int Stars
  }

  MEALPLAN {
    int Id
    int UserId
    string Name
    datetime CreatedOn
  }

  FAVOURITES {
    int Id
    int UserId
    int RecipeId
    bool IsDeleted
    datetime CreatedOn
  }

  SHOPPINGLIST {
    int Id
    int UserId
    string Ingredient
    decimal Quantity
    string Unit
    bool Purchased
  }
```


### Database Migrations

All schema changes are managed with Entity Framework migrations located in `/Migrations`.

```bash
# Create a new migration
dotnet ef migrations add AddShoppingListTable

# Apply latest migrations to the development DB
dotnet ef database update

# Revert to a previous migration
dotnet ef database update <MigrationName>
```

---

## 🧭 Controllers

| Controller | Purpose |
|-------------|----------|
| `HomeController` | Handles homepage and featured recipe display. |
| `RecipeDetailsController` | Displays individual recipe details, ratings, and comments. |
| `MealPlanController` | Handles creation, viewing, and deletion of meal plans. |
| `CommentsController` | Manages comment submissions with authentication guard. |
| `CategoriesController` | Lists recipe categories with carousel view models. |
| `UsersController` | Manages registration and profile settings. |
| `SessionsController` | Handles login, logout, and authentication state. |
| `FavouritesController` | Toggles and manages favourite recipes (soft-delete). |
| `SearchBarController` | Manages complex search logic for recipes, chefs, ingredients, filters. |
| `ShoppingListController` | Handles creation and update of shopping list items. |

---

## 🧠 ViewModels

| ViewModel | Description |
|------------|--------------|
| `HomePageViewModel` | Displays featured, recent, and category carousels. |
| `RecipeCardViewModel` | Represents individual recipe card data. |
| `RecipeCarouselViewModel` | Groups multiple recipes for carousel rendering. |
| `IndividualRecipeViewModel` | Core data for single recipe display. |
| `IndividualRecipePageViewModel` | Combines recipe details, comments, and ratings. |
| `MealPlanViewModel` | Represents a single meal plan. |
| `MealPlansPageViewModel` | Aggregates all meal plans for a user. |
| `MealPlanRecipeViewModel` | Represents individual recipes within a meal plan. |
| `SignInViewModel` / `SignUpViewModel` | User authentication models. |
| `CommentingViewModel` | Handles comment creation and validation. |
| `ErrorViewModel` | Displays handled exceptions gracefully. |
| `SearchPageViewModel` | Manages all search filter data and resulting recipe cards. |
| `CategoryPageViewModel` / `CategoryCarouselViewModel` | Used for category listing and display. |
| `AccountSettingsViewModel` | Aggregates account management forms. |
| `ChangeUsernameViewModel`, `ChangeEmailViewModel`, `ChangePasswordViewModel`, `DeleteAccountViewModel` | Sub-viewmodels for profile management. |
| `ShoppingListViewModel` | Represents individual list items and purchased flags. |
| `FavouritesViewModel` / `FavouritesPageViewModel` | Displays and manages favourite recipes. |
| `AccountSettingsViewModel` | Includes nested models for user profile updates. |

---

## 🧱 Views & Partials

| File | Function |
|------|-----------|
| `Views/Home/Index.cshtml` | Homepage layout with featured sections. |
| `Views/RecipeDetails/Index.cshtml` | Recipe display with ingredients, comments, and star ratings. |
| `Views/MealPlan/Index.cshtml` / `Show.cshtml` | Meal planning interface and detail view. |
| `Views/SearchBar/Index.cshtml` | Full search interface with dropdown filters. |
| `Views/Favourites/Index.cshtml` | Lists all saved recipes per user. |
| `Views/ShoppingList/Show.cshtml` | Displays generated shopping list. |
| `Views/Shared/_Layout.cshtml` | Global layout (navbar, footer, dark/light mode). |
| `Views/Shared/_RecipeCardVerticalPartial.cshtml` / `_RecipeCardHorizontalPartial.cshtml` | Recipe card UI. |
| `Views/Shared/_FeaturedRecipePartial.cshtml` | Featured recipe section. |
| `Views/Shared/_FavouritesRecipeCardPartial.cshtml` | Favourite recipe listing. |
| `Views/Shared/_ShoppingListPartial.cshtml` | Shopping list block in meal planner. |

---

## 💻 Frontend JavaScript

| Script | Purpose |
|---------|----------|
| `navbar.js` | Controls responsive navigation and expanding search bar. |
| `searchpage.js` | Toggles dropdown filters (category, chef, ingredient, duration). |
| `favouritespage.js` | Animates success/failure alerts on favourites page. |
| `backtotopbutton.js` | Shows scroll-to-top button dynamically. |
| `homepagemealplanbutton.js` | Links homepage quick meal plan button. |
| `recipeDetails.js` | Handles star ratings and comment interactivity. |
| `carousel.js` | Enables auto-scrolling category and recipe carousels. |
| `site.js` | General-purpose site scripts. |

---

## 🧪 Testing (xUnit + Playwright)

SueChef uses **Playwright** for UI automation and **xUnit** for integration and unit testing.

| Test File | Coverage |
|------------|-----------|
| `HomePage.Test.cs` | Ensures homepage loads with featured recipes and nav. |
| `CategoryPage.Test.cs` | Validates category page filtering and layout. |
| `MealPlansPage.Test.cs` | Confirms all meal plan cards and navigation. |
| `SearchBar.Test.cs` | Tests search filters — keyword, category, chef, difficulty, duration. |
| `TestIndividualPage.cs` | Verifies recipe detail elements (title, comments, ratings). |
| `RecipeCardViewModel.Test.cs` | Validates viewmodel binding to partial views. |
| `Test For SignIn and SignUp.cs` | Tests login, signup validation, and redirects. |
| `UnitTest1.cs` | Generic baseline test runner. |
| `TestDataSeeder.cs` | Seeds consistent test data for integration. |
| `DbFactory.cs` | Configures and injects in-memory test database context. |

### Running the Tests

```bash
# install playwright dependencies
npx playwright install

# run tests
dotnet test
npx playwright test
```

### Troubleshooting Playwright

- If browsers not found → run `npx playwright install` again.  
- If timeouts occur → increase timeout using `.ToBeVisibleAsync(new() { Timeout = 10000 })`.  
- For flaky selectors → add `await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);` before assertions.

---


## 🙏 Acknowledgements & Documentation

| Technology | Documentation | Purpose |
|-------------|----------------|----------|
| ASP.NET Core MVC | [ASP.NET Core MVC Overview](https://learn.microsoft.com/en-us/aspnet/core/mvc/overview?view=aspnetcore-9.0) | Framework for controllers & routing |
| Entity Framework Core | [EF Core Docs](https://learn.microsoft.com/en-us/ef/core/) | ORM and migrations |
| Razor Views | [Razor Syntax Reference](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/razor?view=aspnetcore-9.0) | Template rendering |
| Tailwind CSS | [Tailwind Docs](https://tailwindcss.com/docs) | CSS utility framework |
| Google OAuth | [Google Identity Docs](https://developers.google.com/identity) | Third-party authentication |
| xUnit | [xUnit Docs](https://xunit.net/) | Testing framework |
| Playwright | [Playwright Docs](https://playwright.dev/dotnet/docs/intro) | Browser automation |
| .NET CLI | [.NET CLI Docs](https://learn.microsoft.com/en-us/dotnet/core/tools/) | Build and run commands |
| LINQ | [LINQ Docs](https://learn.microsoft.com/en-us/dotnet/csharp/linq/) | Data querying |
| C# Language | [C# Reference](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/) | Language documentation |

---

## 🧭 Summary of Documentation

| Category | Documentation | Use Case |
|-----------|----------------|----------|
| Core Framework | ASP.NET Core MVC | Controllers, routing, and views |
| Database | Entity Framework Core | Data relationships and migrations |
| Frontend | Tailwind CSS + Razor | Styling and templates |
| Authentication | ASP.NET Sessions + Google OAuth | User login and management |
| Testing | xUnit + Playwright | Testing coverage |
| Deployment | .NET CLI | Build automation and hosting |
| Language | C# + LINQ | Syntax and data logic |

---

## 🧾 License

This project is released under the MIT License.


© 2025 SueChef — Makers Academy Final Project
