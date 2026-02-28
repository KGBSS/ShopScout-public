using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopScout.Data.Migrations
{
    /// <inheritdoc />
    public partial class changes_to_products_and_stores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Changes_Products_ProductId",
                table: "Changes");

            migrationBuilder.DropForeignKey(
                name: "FK_Changes_Stores_StoreId",
                table: "Changes");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Changes",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Changes_Products_ProductId",
                table: "Changes",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Changes_Stores_StoreId",
                table: "Changes",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Changes_Products_ProductId",
                table: "Changes");

            migrationBuilder.DropForeignKey(
                name: "FK_Changes_Stores_StoreId",
                table: "Changes");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Changes");

            migrationBuilder.AddForeignKey(
                name: "FK_Changes_Products_ProductId",
                table: "Changes",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Changes_Stores_StoreId",
                table: "Changes",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id");
        }
    }
}
