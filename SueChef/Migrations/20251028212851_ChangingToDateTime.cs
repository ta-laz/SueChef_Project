using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SueChef.Migrations
{
    /// <inheritdoc />
    public partial class ChangingToDateTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Change columns from DateOnly (date) to DateTime (datetime2)
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedOn",
                table: "MealPlans",
                type: "timestamp without time zone",  // For PostgreSQL
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOn",
                table: "MealPlans",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            // Preserve existing data and add 00:01 time
            migrationBuilder.Sql("UPDATE \"MealPlans\" SET \"UpdatedOn\" = \"UpdatedOn\" + INTERVAL '1 minute' WHERE \"UpdatedOn\" IS NOT NULL;");
            migrationBuilder.Sql("UPDATE \"MealPlans\" SET \"CreatedOn\" = \"CreatedOn\" + INTERVAL '1 minute' WHERE \"CreatedOn\" IS NOT NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateOnly>(
                name: "UpdatedOn",
                table: "MealPlans",
                type: "date",
                nullable: true,
                defaultValueSql: "CURRENT_DATE",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "CreatedOn",
                table: "MealPlans",
                type: "date",
                nullable: true,
                defaultValueSql: "CURRENT_DATE",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "CURRENT_TIMESTAMP");
        }
    }
}
