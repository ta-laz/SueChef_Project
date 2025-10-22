using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SueChef.Migrations
{
    /// <inheritdoc />
    public partial class PrepCookTimes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CookTime",
                table: "Recipes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PrepTime",
                table: "Recipes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CookTime",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "PrepTime",
                table: "Recipes");
        }
    }
}
