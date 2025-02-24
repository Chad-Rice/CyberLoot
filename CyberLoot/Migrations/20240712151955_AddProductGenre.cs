using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberLoot.Migrations
{
    /// <inheritdoc />
    public partial class AddProductGenre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Genres_GenreId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_GenreId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "GenreId",
                table: "Products");

            migrationBuilder.CreateTable(
                name: "ProductGenres",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    GenreId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductGenres", x => new { x.ProductId, x.GenreId });
                    table.ForeignKey(
                        name: "FK_ProductGenres_Genres_GenreId",
                        column: x => x.GenreId,
                        principalTable: "Genres",
                        principalColumn: "GenreId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductGenres_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductGenres_GenreId",
                table: "ProductGenres",
                column: "GenreId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductGenres");

            migrationBuilder.AddColumn<int>(
                name: "GenreId",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Products_GenreId",
                table: "Products",
                column: "GenreId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Genres_GenreId",
                table: "Products",
                column: "GenreId",
                principalTable: "Genres",
                principalColumn: "GenreId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
