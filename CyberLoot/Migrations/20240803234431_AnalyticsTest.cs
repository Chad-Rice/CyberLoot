using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberLoot.Migrations
{
    /// <inheritdoc />
    public partial class AnalyticsTest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "WishlistItems",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "Analytics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VisitDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PageVisited = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Analytics", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WishlistItems_UserId",
                table: "WishlistItems",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_WishlistItems_AspNetUsers_UserId",
                table: "WishlistItems",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WishlistItems_AspNetUsers_UserId",
                table: "WishlistItems");

            migrationBuilder.DropTable(
                name: "Analytics");

            migrationBuilder.DropIndex(
                name: "IX_WishlistItems_UserId",
                table: "WishlistItems");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "WishlistItems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
