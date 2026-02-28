using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopScout.Data.Migrations
{
    /// <inheritdoc />
    public partial class details : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Fat",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IngredientsText",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "NovaGroup",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "NutriScore",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Salt",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SaturatedFat",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ServingSize",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Sugars",
                table: "Products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "Fat",
                table: "Products",
                type: "tinyint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IngredientsText",
                table: "Products",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NovaGroup",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "NutriScore",
                table: "Products",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Quantity",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "Salt",
                table: "Products",
                type: "tinyint",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "SaturatedFat",
                table: "Products",
                type: "tinyint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServingSize",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "Sugars",
                table: "Products",
                type: "tinyint",
                nullable: true);
        }
    }
}
