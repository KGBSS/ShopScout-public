using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopScout.Data.Migrations
{
    /// <inheritdoc />
    public partial class product_on_shelf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FavoriteProductPushEnabled",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UpdateEmailEnabled",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UpdatePushEnabled",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<byte>(
                name: "Type",
                table: "StoreAttributes",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<float>(
                name: "DistanceFromP1",
                table: "ProductPerStore",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShelfId",
                table: "ProductPerStore",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "NovaGroup",
                table: "ProductDetails",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPerStore_ShelfId",
                table: "ProductPerStore",
                column: "ShelfId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPerStore_Shelves_ShelfId",
                table: "ProductPerStore",
                column: "ShelfId",
                principalTable: "Shelves",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductPerStore_Shelves_ShelfId",
                table: "ProductPerStore");

            migrationBuilder.DropIndex(
                name: "IX_ProductPerStore_ShelfId",
                table: "ProductPerStore");

            migrationBuilder.DropColumn(
                name: "DistanceFromP1",
                table: "ProductPerStore");

            migrationBuilder.DropColumn(
                name: "ShelfId",
                table: "ProductPerStore");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "StoreAttributes",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<int>(
                name: "NovaGroup",
                table: "ProductDetails",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AddColumn<bool>(
                name: "FavoriteProductPushEnabled",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UpdateEmailEnabled",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UpdatePushEnabled",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
