using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopScout.Data.Migrations
{
    /// <inheritdoc />
    public partial class arfigyelo_v1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Stores",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OpeningHours",
                table: "Stores",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Verified",
                table: "Stores",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Verified",
                table: "StoreBrands",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "OpeningHours",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "Verified",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "Verified",
                table: "StoreBrands");
        }
    }
}
