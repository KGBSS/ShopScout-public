using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopScout.Data.Migrations
{
    /// <inheritdoc />
    public partial class store_namefix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductStore_StoreLocations_StoresId",
                table: "ProductStore");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StoreLocations",
                table: "StoreLocations");

            migrationBuilder.RenameTable(
                name: "StoreLocations",
                newName: "Stores");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Stores",
                table: "Stores",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductStore_Stores_StoresId",
                table: "ProductStore",
                column: "StoresId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductStore_Stores_StoresId",
                table: "ProductStore");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Stores",
                table: "Stores");

            migrationBuilder.RenameTable(
                name: "Stores",
                newName: "StoreLocations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StoreLocations",
                table: "StoreLocations",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductStore_StoreLocations_StoresId",
                table: "ProductStore",
                column: "StoresId",
                principalTable: "StoreLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
