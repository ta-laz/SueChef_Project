using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SueChef.Migrations
{
    /// <inheritdoc />
    public partial class fixedratingtable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ratings_Recipes_UserId",
                table: "Ratings");

            migrationBuilder.DropForeignKey(
                name: "FK_Ratings_Users_UsersId",
                table: "Ratings");

            migrationBuilder.DropIndex(
                name: "IX_Ratings_UsersId",
                table: "Ratings");

            migrationBuilder.DropColumn(
                name: "UsersId",
                table: "Ratings");

            migrationBuilder.RenameColumn(
                name: "stars",
                table: "Ratings",
                newName: "Stars");

            migrationBuilder.RenameColumn(
                name: "RatingId",
                table: "Ratings",
                newName: "Id");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Ratings",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Ratings_Recipes_UserId",
                table: "Ratings",
                column: "UserId",
                principalTable: "Recipes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ratings_Recipes_UserId",
                table: "Ratings");

            migrationBuilder.RenameColumn(
                name: "Stars",
                table: "Ratings",
                newName: "stars");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Ratings",
                newName: "RatingId");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Ratings",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsersId",
                table: "Ratings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_UsersId",
                table: "Ratings",
                column: "UsersId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ratings_Recipes_UserId",
                table: "Ratings",
                column: "UserId",
                principalTable: "Recipes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ratings_Users_UsersId",
                table: "Ratings",
                column: "UsersId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
