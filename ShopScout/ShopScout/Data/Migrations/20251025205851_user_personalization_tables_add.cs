using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopScout.Data.Migrations
{
    /// <inheritdoc />
    public partial class user_personalization_tables_add : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Stores",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ProductCategories",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Cities",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Cities",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FavoriteProductPushEnabled",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAnonymous",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LanguageCode",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.CreateTable(
                name: "ApplicationUserProductCategory",
                columns: table => new
                {
                    FavoriteCategoriesId = table.Column<int>(type: "int", nullable: false),
                    UsersId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUserProductCategory", x => new { x.FavoriteCategoriesId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_ApplicationUserProductCategory_AspNetUsers_UsersId",
                        column: x => x.UsersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationUserProductCategory_ProductCategories_FavoriteCategoriesId",
                        column: x => x.FavoriteCategoriesId,
                        principalTable: "ProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Stores_ApplicationUserId",
                table: "Stores",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_Name",
                table: "ProductCategories",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_ApplicationUserId",
                table: "Cities",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_Name",
                table: "Cities",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserProductCategory_UsersId",
                table: "ApplicationUserProductCategory",
                column: "UsersId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_AspNetUsers_ApplicationUserId",
                table: "Cities",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Stores_AspNetUsers_ApplicationUserId",
                table: "Stores",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cities_AspNetUsers_ApplicationUserId",
                table: "Cities");

            migrationBuilder.DropForeignKey(
                name: "FK_Stores_AspNetUsers_ApplicationUserId",
                table: "Stores");

            migrationBuilder.DropTable(
                name: "ApplicationUserProductCategory");

            migrationBuilder.DropIndex(
                name: "IX_Stores_ApplicationUserId",
                table: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_ProductCategories_Name",
                table: "ProductCategories");

            migrationBuilder.DropIndex(
                name: "IX_Cities_ApplicationUserId",
                table: "Cities");

            migrationBuilder.DropIndex(
                name: "IX_Cities_Name",
                table: "Cities");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Cities");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FavoriteProductPushEnabled",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsAnonymous",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LanguageCode",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UpdateEmailEnabled",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UpdatePushEnabled",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ProductCategories",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Cities",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
