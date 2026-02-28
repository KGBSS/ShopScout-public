using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopScout.Data.Migrations
{
    /// <inheritdoc />
    public partial class productperstore_v3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArfigyeloId",
                table: "Products");

            migrationBuilder.AddColumn<bool>(
                name: "FromArfigyelo",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FromArfigyelo",
                table: "Products");

            migrationBuilder.AddColumn<string>(
                name: "ArfigyeloId",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
