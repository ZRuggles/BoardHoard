using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoardHoardAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdatingAPI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BoardGames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    MinPlayers = table.Column<int>(type: "int", nullable: false),
                    MaxPlayers = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Publisher = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ageRating = table.Column<int>(type: "int", nullable: true),
                    Ratings = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RarityScore = table.Column<double>(type: "float(6)", precision: 6, scale: 2, nullable: false),
                    SentimentalScore = table.Column<double>(type: "float(6)", precision: 6, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardGames", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CardGames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    MinPlayers = table.Column<int>(type: "int", nullable: false),
                    MaxPlayers = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Publisher = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ageRating = table.Column<int>(type: "int", nullable: true),
                    Ratings = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RarityScore = table.Column<double>(type: "float(6)", precision: 6, scale: 2, nullable: false),
                    SentimentalScore = table.Column<double>(type: "float(6)", precision: 6, scale: 2, nullable: false),
                    cardCount = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardGames", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BoardGames_Title",
                table: "BoardGames",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_CardGames_Title",
                table: "CardGames",
                column: "Title");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BoardGames");

            migrationBuilder.DropTable(
                name: "CardGames");
        }
    }
}
