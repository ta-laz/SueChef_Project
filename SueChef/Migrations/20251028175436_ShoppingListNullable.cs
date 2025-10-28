using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SueChef.Migrations
{
    /// <inheritdoc />
    public partial class ShoppingListNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Convert text -> numeric, safely turning empty strings into NULL
        migrationBuilder.Sql("""
            ALTER TABLE "ShoppingLists"
            ALTER COLUMN "AdditionalQuantity"
            TYPE numeric(18,2)
            USING NULLIF(trim("AdditionalQuantity"), '')::numeric;
        """);
    }

        /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Convert numeric -> text
        migrationBuilder.Sql("""
            ALTER TABLE "ShoppingLists"
            ALTER COLUMN "AdditionalQuantity"
            TYPE text
            USING "AdditionalQuantity"::text;
        """);
    }
    }
}
