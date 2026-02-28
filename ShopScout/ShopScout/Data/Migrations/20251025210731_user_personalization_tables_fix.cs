using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopScout.Data.Migrations
{
    /// <inheritdoc />
    public partial class user_personalization_tables_fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cities_AspNetUsers_ApplicationUserId",
                table: "Cities");

            migrationBuilder.DropIndex(
                name: "IX_Cities_ApplicationUserId",
                table: "Cities");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Cities");

            migrationBuilder.CreateTable(
                name: "ApplicationUserCity",
                columns: table => new
                {
                    FavoriteCitiesId = table.Column<int>(type: "int", nullable: false),
                    UsersId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUserCity", x => new { x.FavoriteCitiesId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_ApplicationUserCity_AspNetUsers_UsersId",
                        column: x => x.UsersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationUserCity_Cities_FavoriteCitiesId",
                        column: x => x.FavoriteCitiesId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserCity_UsersId",
                table: "ApplicationUserCity",
                column: "UsersId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationUserCity");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Cities",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cities_ApplicationUserId",
                table: "Cities",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_AspNetUsers_ApplicationUserId",
                table: "Cities",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
