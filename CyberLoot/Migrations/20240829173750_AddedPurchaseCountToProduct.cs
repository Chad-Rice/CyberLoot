using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberLoot.Migrations
{
    /// <inheritdoc />
    public partial class AddedPurchaseCountToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PurchaseCount",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PurchaseCount",
                table: "Products");
        }
    }
}
