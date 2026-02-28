using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopScout.Data.Migrations
{
    /// <inheritdoc />
    public partial class arfigyelo_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductPrices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StoreBrandId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductPriceStore");

            migrationBuilder.DropTable(
                name: "ProductPrices");
        }
    }
}
