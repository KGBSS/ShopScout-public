using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopScout.Data.Migrations
{
    /// <inheritdoc />
    public partial class changes_to_products_and_stores_with_enum_fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Changes");

            migrationBuilder.AddColumn<byte>(
                name: "ChangeEntityType",
                table: "Changes",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChangeEntityType",
                table: "Changes");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Changes",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");
        }
    }
}
