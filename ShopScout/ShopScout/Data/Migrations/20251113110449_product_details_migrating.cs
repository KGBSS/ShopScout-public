using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopScout.Data.Migrations
{
    /// <inheritdoc />
    public partial class product_details_migrating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Stores_StoreId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_StoreId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "Products");

            migrationBuilder.CreateTable(
                name: "ProductDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", maxLength: 100, nullable: false),
                    Quantity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IngredientsText = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    NutriScore = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NovaGroup = table.Column<int>(type: "int", nullable: false),
                    EnergyKcal = table.Column<double>(type: "float", nullable: true),
                    Fat = table.Column<double>(type: "float", nullable: true),
                    SaturatedFat = table.Column<double>(type: "float", nullable: true),
                    Carbohydrates = table.Column<double>(type: "float", nullable: true),
                    Sugars = table.Column<double>(type: "float", nullable: true),
                    Proteins = table.Column<double>(type: "float", nullable: true),
                    Salt = table.Column<double>(type: "float", nullable: true),
                    Fiber = table.Column<double>(type: "float", nullable: true),
                    Sodium = table.Column<double>(type: "float", nullable: true),
                    ServingSize = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FatLevel = table.Column<byte>(type: "tinyint", nullable: true),
                    SaturatedFatLevel = table.Column<byte>(type: "tinyint", nullable: true),
                    SaltLevel = table.Column<byte>(type: "tinyint", nullable: true),
                    SugarsLevel = table.Column<byte>(type: "tinyint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductDetails_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductDetails_ProductId",
                table: "ProductDetails",
                column: "ProductId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductDetails");

            migrationBuilder.AddColumn<int>(
                name: "StoreId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_StoreId",
                table: "Products",
                column: "StoreId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Stores_StoreId",
                table: "Products",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id");
        }
    }
}
