using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopScout.Data.Migrations
{
    /// <inheritdoc />
    public partial class layout_in_db : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Layout",
                table: "Stores");

            migrationBuilder.CreateTable(
                name: "LayoutObjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntranceX1 = table.Column<int>(type: "int", nullable: false),
                    EntranceY1 = table.Column<int>(type: "int", nullable: false),
                    EntranceX2 = table.Column<int>(type: "int", nullable: false),
                    EntranceY2 = table.Column<int>(type: "int", nullable: false),
                    StoreId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LayoutObjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LayoutObjects_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Shelves",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    X1 = table.Column<int>(type: "int", nullable: false),
                    Y1 = table.Column<int>(type: "int", nullable: false),
                    X2 = table.Column<int>(type: "int", nullable: false),
                    Y2 = table.Column<int>(type: "int", nullable: false),
                    LayoutObjectId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shelves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shelves_LayoutObjects_LayoutObjectId",
                        column: x => x.LayoutObjectId,
                        principalTable: "LayoutObjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Walls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    X1 = table.Column<int>(type: "int", nullable: false),
                    Y1 = table.Column<int>(type: "int", nullable: false),
                    X2 = table.Column<int>(type: "int", nullable: false),
                    Y2 = table.Column<int>(type: "int", nullable: false),
                    LayoutObjectId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Walls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Walls_LayoutObjects_LayoutObjectId",
                        column: x => x.LayoutObjectId,
                        principalTable: "LayoutObjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LayoutObjects_StoreId",
                table: "LayoutObjects",
                column: "StoreId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shelves_LayoutObjectId",
                table: "Shelves",
                column: "LayoutObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Walls_LayoutObjectId",
                table: "Walls",
                column: "LayoutObjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Shelves");

            migrationBuilder.DropTable(
                name: "Walls");

            migrationBuilder.DropTable(
                name: "LayoutObjects");

            migrationBuilder.AddColumn<string>(
                name: "Layout",
                table: "Stores",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
