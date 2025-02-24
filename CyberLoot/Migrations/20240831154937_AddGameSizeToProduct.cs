using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberLoot.Migrations
{
    /// <inheritdoc />
    public partial class AddGameSizeToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GameSize",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GameSize",
                table: "Products");
        }
    }
}
