using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopScout.Data.Migrations
{
    /// <inheritdoc />
    public partial class product_details_migrating_v1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Fiber",
                table: "ProductDetails");

            migrationBuilder.DropColumn(
                name: "Sodium",
                table: "ProductDetails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Fiber",
                table: "ProductDetails",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Sodium",
                table: "ProductDetails",
                type: "float",
                nullable: true);
        }
    }
}
