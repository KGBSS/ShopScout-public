using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopScout.Data.Migrations
{
    /// <inheritdoc />
    public partial class nutriscore_to_number : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NutriScore",
                table: "ProductDetails",
                newName: "NutriScoreString");

            migrationBuilder.AddColumn<byte>(
                name: "NutriScore",
                table: "ProductDetails",
                type: "tinyint",
                maxLength: 50,
                nullable: false,
                defaultValue: (byte)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NutriScore",
                table: "ProductDetails");

            migrationBuilder.RenameColumn(
                name: "NutriScoreString",
                table: "ProductDetails",
                newName: "NutriScore");
        }
    }
}
