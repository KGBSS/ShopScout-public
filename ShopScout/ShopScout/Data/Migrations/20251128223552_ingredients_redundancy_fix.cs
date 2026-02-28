using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopScout.Data.Migrations
{
    /// <inheritdoc />
    public partial class ingredients_redundancy_fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductIngredientsTemp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductIngredientsTemp", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductProductIngredient",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    IngredientId = table.Column<int>(type: "int", nullable: false),
                    PercentEstimate = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductProductIngredient", x => new { x.ProductId, x.IngredientId });
                    table.ForeignKey(
                        name: "FK_ProductProductIngredient_ProductIngredientsTemp_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "ProductIngredientsTemp",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductProductIngredient_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductProductIngredient_IngredientId",
                table: "ProductProductIngredient",
                column: "IngredientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductProductIngredient");

            migrationBuilder.DropTable(
                name: "ProductIngredientsTemp");
        }
    }
}
