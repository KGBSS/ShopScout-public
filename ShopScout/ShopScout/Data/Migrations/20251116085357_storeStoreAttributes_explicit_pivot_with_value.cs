using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopScout.Data.Migrations
{
    /// <inheritdoc />
    public partial class storeStoreAttributes_explicit_pivot_with_value : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoreStoreAttribute_StoreAttributes_StoreAttributesId",
                table: "StoreStoreAttribute");

            migrationBuilder.DropForeignKey(
                name: "FK_StoreStoreAttribute_Stores_StoresId",
                table: "StoreStoreAttribute");

            migrationBuilder.RenameColumn(
                name: "StoresId",
                table: "StoreStoreAttribute",
                newName: "StoreId");

            migrationBuilder.RenameColumn(
                name: "StoreAttributesId",
                table: "StoreStoreAttribute",
                newName: "StoreAttributeId");

            migrationBuilder.RenameIndex(
                name: "IX_StoreStoreAttribute_StoresId",
                table: "StoreStoreAttribute",
                newName: "IX_StoreStoreAttribute_StoreId");

            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "StoreStoreAttribute",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StoreStoreAttribute_StoreAttributes_StoreAttributeId",
                table: "StoreStoreAttribute",
                column: "StoreAttributeId",
                principalTable: "StoreAttributes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StoreStoreAttribute_Stores_StoreId",
                table: "StoreStoreAttribute",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoreStoreAttribute_StoreAttributes_StoreAttributeId",
                table: "StoreStoreAttribute");

            migrationBuilder.DropForeignKey(
                name: "FK_StoreStoreAttribute_Stores_StoreId",
                table: "StoreStoreAttribute");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "StoreStoreAttribute");

            migrationBuilder.RenameColumn(
                name: "StoreId",
                table: "StoreStoreAttribute",
                newName: "StoresId");

            migrationBuilder.RenameColumn(
                name: "StoreAttributeId",
                table: "StoreStoreAttribute",
                newName: "StoreAttributesId");

            migrationBuilder.RenameIndex(
                name: "IX_StoreStoreAttribute_StoreId",
                table: "StoreStoreAttribute",
                newName: "IX_StoreStoreAttribute_StoresId");

            migrationBuilder.AddForeignKey(
                name: "FK_StoreStoreAttribute_StoreAttributes_StoreAttributesId",
                table: "StoreStoreAttribute",
                column: "StoreAttributesId",
                principalTable: "StoreAttributes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StoreStoreAttribute_Stores_StoresId",
                table: "StoreStoreAttribute",
                column: "StoresId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
