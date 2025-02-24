using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberLoot.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedWishLists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MaxPrice",
                table: "WishlistItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MinPrice",
                table: "WishlistItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxPrice",
                table: "WishlistItems");

            migrationBuilder.DropColumn(
                name: "MinPrice",
                table: "WishlistItems");
        }
    }
}
