using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopScout.Data.Migrations
{
    /// <inheritdoc />
    public partial class db_fix_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_PackagingMaterials_PackagingMaterialId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_PackagingParts_PackagingPartId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_PackagingMaterialId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_PackagingPartId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PackagingMaterialId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PackagingPartId",
                table: "Products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PackagingMaterialId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PackagingPartId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_PackagingMaterialId",
                table: "Products",
                column: "PackagingMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_PackagingPartId",
                table: "Products",
                column: "PackagingPartId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_PackagingMaterials_PackagingMaterialId",
                table: "Products",
                column: "PackagingMaterialId",
                principalTable: "PackagingMaterials",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_PackagingParts_PackagingPartId",
                table: "Products",
                column: "PackagingPartId",
                principalTable: "PackagingParts",
                principalColumn: "Id");
        }
    }
}
