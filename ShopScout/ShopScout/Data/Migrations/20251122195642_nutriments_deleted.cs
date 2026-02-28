using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopScout.Data.Migrations
{
    /// <inheritdoc />
    public partial class nutriments_deleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Nutriments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Nutriments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Carbohydrates = table.Column<double>(type: "float", nullable: true),
                    EnergyKcal = table.Column<double>(type: "float", nullable: true),
                    Fat = table.Column<double>(type: "float", nullable: true),
                    Fiber = table.Column<double>(type: "float", nullable: true),
                    Proteins = table.Column<double>(type: "float", nullable: true),
                    Salt = table.Column<double>(type: "float", nullable: true),
                    SaturatedFat = table.Column<double>(type: "float", nullable: true),
                    Sodium = table.Column<double>(type: "float", nullable: true),
                    Sugars = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nutriments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Nutriments_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Nutriments_ProductId",
                table: "Nutriments",
                column: "ProductId",
                unique: true);
        }
    }
}
