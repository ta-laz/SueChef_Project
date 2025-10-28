using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SueChef.Migrations
{
    /// <inheritdoc />
    public partial class FixMealPlanDateTimeUtc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedOn",
                table: "MealPlans",
                type: "timestamptz",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOn",
                table: "MealPlans",
                type: "timestamptz",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "CURRENT_TIMESTAMP");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedOn",
                table: "MealPlans",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamptz",
                oldNullable: true,
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOn",
                table: "MealPlans",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamptz",
                oldNullable: true,
                oldDefaultValueSql: "CURRENT_TIMESTAMP");
        }
    }
}
