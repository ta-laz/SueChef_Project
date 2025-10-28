using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SueChef.Migrations
{
    /// <inheritdoc />
    public partial class RedoingShoppingList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingLists_RecipeIngredients_RecipeIngredientId",
                table: "ShoppingLists");

            migrationBuilder.DropIndex(
                name: "IX_ShoppingLists_RecipeIngredientId",
                table: "ShoppingLists");

            migrationBuilder.DropColumn(
                name: "RecipeIngredientId",
                table: "ShoppingLists");

            migrationBuilder.DropColumn(
                name: "Servings",
                table: "ShoppingLists");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "ShoppingLists",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IngredientName",
                table: "ShoppingLists",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                table: "ShoppingLists",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "ShoppingLists",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "ShoppingLists");

            migrationBuilder.DropColumn(
                name: "IngredientName",
                table: "ShoppingLists");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "ShoppingLists");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "ShoppingLists");

            migrationBuilder.AddColumn<int>(
                name: "RecipeIngredientId",
                table: "ShoppingLists",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Servings",
                table: "ShoppingLists",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingLists_RecipeIngredientId",
                table: "ShoppingLists",
                column: "RecipeIngredientId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingLists_RecipeIngredients_RecipeIngredientId",
                table: "ShoppingLists",
                column: "RecipeIngredientId",
                principalTable: "RecipeIngredients",
                principalColumn: "Id");
        }
    }
}
