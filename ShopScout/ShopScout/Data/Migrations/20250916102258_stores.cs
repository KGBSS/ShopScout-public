using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopScout.Data.Migrations
{
    /// <inheritdoc />
    public partial class stores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_StoreLocations_StoreLocationId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_StoreLocationId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "StoreLocationId",
                table: "Products");

            migrationBuilder.CreateTable(
                name: "ProductStore",
                columns: table => new
                {
                    ProductsId = table.Column<int>(type: "int", nullable: false),
                    StoresId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductStore", x => new { x.ProductsId, x.StoresId });
                    table.ForeignKey(
                        name: "FK_ProductStore_Products_ProductsId",
                        column: x => x.ProductsId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductStore_StoreLocations_StoresId",
                        column: x => x.StoresId,
                        principalTable: "StoreLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductStore_StoresId",
                table: "ProductStore",
                column: "StoresId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductStore");

            migrationBuilder.AddColumn<int>(
                name: "StoreLocationId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_StoreLocationId",
                table: "Products",
                column: "StoreLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_StoreLocations_StoreLocationId",
                table: "Products",
                column: "StoreLocationId",
                principalTable: "StoreLocations",
                principalColumn: "Id");
        }
    }
}
