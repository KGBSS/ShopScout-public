using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopScout.Data.Migrations
{
    /// <inheritdoc />
    public partial class changes_and_reviewqueue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Changes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ChangeJson = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    ChangeRequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    StoreId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Changes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Changes_AspNetUsers_ApprovedById",
                        column: x => x.ApprovedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Changes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Changes_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Changes_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReviewQueue",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChangeId = table.Column<int>(type: "int", nullable: false),
                    RequiredRoleId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewQueue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewQueue_AspNetRoles_RequiredRoleId",
                        column: x => x.RequiredRoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReviewQueue_Changes_ChangeId",
                        column: x => x.ChangeId,
                        principalTable: "Changes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Changes_ApprovedById",
                table: "Changes",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_Changes_ProductId",
                table: "Changes",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Changes_StoreId",
                table: "Changes",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Changes_UserId",
                table: "Changes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewQueue_ChangeId",
                table: "ReviewQueue",
                column: "ChangeId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewQueue_RequiredRoleId",
                table: "ReviewQueue",
                column: "RequiredRoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReviewQueue");

            migrationBuilder.DropTable(
                name: "Changes");
        }
    }
}
