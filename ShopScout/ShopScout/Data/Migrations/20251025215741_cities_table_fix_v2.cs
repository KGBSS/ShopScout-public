using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopScout.Data.Migrations
{
    /// <inheritdoc />
    public partial class cities_table_fix_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "Cities");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "ZipCode",
                table: "Cities",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);
        }
    }
}
