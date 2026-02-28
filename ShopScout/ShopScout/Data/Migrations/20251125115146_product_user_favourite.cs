using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopScout.Data.Migrations
{
    /// <inheritdoc />
    public partial class product_user_favourite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationUserProduct",
                columns: table => new
                {
                    FavoriteProductsId = table.Column<int>(type: "int", nullable: false),
                    FavouritedById = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUserProduct", x => new { x.FavoriteProductsId, x.FavouritedById });
                    table.ForeignKey(
                        name: "FK_ApplicationUserProduct_AspNetUsers_FavouritedById",
                        column: x => x.FavouritedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationUserProduct_Products_FavoriteProductsId",
                        column: x => x.FavoriteProductsId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserProduct_FavouritedById",
                table: "ApplicationUserProduct",
                column: "FavouritedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationUserProduct");
        }
    }
}
