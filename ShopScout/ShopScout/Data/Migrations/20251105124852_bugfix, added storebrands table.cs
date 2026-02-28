using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopScout.Data.Migrations
{
    /// <inheritdoc />
    public partial class bugfixaddedstorebrandstable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stores_AspNetUsers_ApplicationUserId",
                table: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_Stores_ApplicationUserId",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Stores");

            migrationBuilder.AddColumn<int>(
                name: "StoreBrandId",
                table: "Stores",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ApplicationUserStore",
                columns: table => new
                {
                    FavoriteStoresId = table.Column<int>(type: "int", nullable: false),
                    FavoritedById = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUserStore", x => new { x.FavoriteStoresId, x.FavoritedById });
                    table.ForeignKey(
                        name: "FK_ApplicationUserStore_AspNetUsers_FavoritedById",
                        column: x => x.FavoritedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationUserStore_Stores_FavoriteStoresId",
                        column: x => x.FavoriteStoresId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StoreBrands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreBrands", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Stores_StoreBrandId",
                table: "Stores",
                column: "StoreBrandId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserStore_FavoritedById",
                table: "ApplicationUserStore",
                column: "FavoritedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Stores_StoreBrands_StoreBrandId",
                table: "Stores",
                column: "StoreBrandId",
                principalTable: "StoreBrands",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stores_StoreBrands_StoreBrandId",
                table: "Stores");

            migrationBuilder.DropTable(
                name: "ApplicationUserStore");

            migrationBuilder.DropTable(
                name: "StoreBrands");

            migrationBuilder.DropIndex(
                name: "IX_Stores_StoreBrandId",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "StoreBrandId",
                table: "Stores");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Stores",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stores_ApplicationUserId",
                table: "Stores",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Stores_AspNetUsers_ApplicationUserId",
                table: "Stores",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
