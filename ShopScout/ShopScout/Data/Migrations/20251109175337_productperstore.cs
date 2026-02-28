using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopScout.Data.Migrations
{
    /// <inheritdoc />
    public partial class productperstore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductPriceStore");

            migrationBuilder.DropTable(
                name: "ProductStore");

            migrationBuilder.DropTable(
                name: "ProductPrices");

            migrationBuilder.AddColumn<int>(
                name: "StoreId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductPerStore",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    StoreId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<int>(type: "int", nullable: true),
                    DiscountedPrice = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductPerStore", x => new { x.ProductId, x.StoreId });
                    table.ForeignKey(
                        name: "FK_ProductPerStore_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductPerStore_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_StoreId",
                table: "Products",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPerStore_StoreId",
                table: "ProductPerStore",
                column: "StoreId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Stores_StoreId",
                table: "Products",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Stores_StoreId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "ProductPerStore");

            migrationBuilder.DropIndex(
                name: "IX_Products_StoreId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "Products");

            migrationBuilder.CreateTable(
                name: "ProductPrices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    StoreBrandId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductPrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductPrices_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductPrices_StoreBrands_StoreBrandId",
                        column: x => x.StoreBrandId,
                        principalTable: "StoreBrands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                        name: "FK_ProductStore_Stores_StoresId",
                        column: x => x.StoresId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductPriceStore",
                columns: table => new
                {
                    ProductPriceId = table.Column<int>(type: "int", nullable: false),
                    availableInStoresId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductPriceStore", x => new { x.ProductPriceId, x.availableInStoresId });
                    table.ForeignKey(
                        name: "FK_ProductPriceStore_ProductPrices_ProductPriceId",
                        column: x => x.ProductPriceId,
                        principalTable: "ProductPrices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductPriceStore_Stores_availableInStoresId",
                        column: x => x.availableInStoresId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductPrices_ProductId",
                table: "ProductPrices",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPrices_StoreBrandId",
                table: "ProductPrices",
                column: "StoreBrandId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPriceStore_availableInStoresId",
                table: "ProductPriceStore",
                column: "availableInStoresId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductStore_StoresId",
                table: "ProductStore",
                column: "StoresId");
        }
    }
}
