using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SueChef.Migrations
{
    /// <inheritdoc />
    public partial class ShoppingListTypoFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AdditionalQunatity",
                table: "ShoppingLists",
                newName: "AdditionalQuantity");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AdditionalQuantity",
                table: "ShoppingLists",
                newName: "AdditionalQunatity");
        }
    }
}
