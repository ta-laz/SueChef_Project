using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SueChef.Migrations
{
    /// <inheritdoc />
    public partial class RemoveServings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Serving",
                table: "Recipes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Serving",
                table: "Recipes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
