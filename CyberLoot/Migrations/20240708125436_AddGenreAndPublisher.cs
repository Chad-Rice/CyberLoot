using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberLoot.Migrations
{
    /// <inheritdoc />
    public partial class AddGenreAndPublisher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Publisher",
                table: "Products");

            migrationBuilder.AddColumn<int>(
                name: "GenreId",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PublisherId",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Genres",
                columns: table => new
                {
                    GenreId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GenreName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genres", x => x.GenreId);
                });

            migrationBuilder.CreateTable(
                name: "Publishers",
                columns: table => new
                {
                    PublisherId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PublisherName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Publishers", x => x.PublisherId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_GenreId",
                table: "Products",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_PublisherId",
                table: "Products",
                column: "PublisherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Genres_GenreId",
                table: "Products",
                column: "GenreId",
                principalTable: "Genres",
                principalColumn: "GenreId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Publishers_PublisherId",
                table: "Products",
                column: "PublisherId",
                principalTable: "Publishers",
                principalColumn: "PublisherId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Genres_GenreId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Publishers_PublisherId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "Genres");

            migrationBuilder.DropTable(
                name: "Publishers");

            migrationBuilder.DropIndex(
                name: "IX_Products_GenreId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_PublisherId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "GenreId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PublisherId",
                table: "Products");

            migrationBuilder.AddColumn<string>(
                name: "Publisher",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
