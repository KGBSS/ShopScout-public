using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopScout.Data.Migrations
{
    /// <inheritdoc />
    public partial class store_extra_infos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "ProductIngredients");

            migrationBuilder.RenameColumn(
                name: "GenericName",
                table: "Products",
                newName: "Description");

            migrationBuilder.AddColumn<long>(
                name: "OsmId",
                table: "Stores",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "StoreAttributes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreAttributes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StoreStoreAttribute",
                columns: table => new
                {
                    StoreAttributesId = table.Column<int>(type: "int", nullable: false),
                    StoresId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreStoreAttribute", x => new { x.StoreAttributesId, x.StoresId });
                    table.ForeignKey(
                        name: "FK_StoreStoreAttribute_StoreAttributes_StoreAttributesId",
                        column: x => x.StoreAttributesId,
                        principalTable: "StoreAttributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StoreStoreAttribute_Stores_StoresId",
                        column: x => x.StoresId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StoreStoreAttribute_StoresId",
                table: "StoreStoreAttribute",
                column: "StoresId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StoreStoreAttribute");

            migrationBuilder.DropTable(
                name: "StoreAttributes");

            migrationBuilder.DropColumn(
                name: "OsmId",
                table: "Stores");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Products",
                newName: "GenericName");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Products",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Products",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "ProductIngredients",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
