using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoardHoard.Migrations
{
    /// <inheritdoc />
    public partial class TopGames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TopGamesJson",
                table: "UserProfiles",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TopGamesJson",
                table: "UserProfiles");
        }
    }
}
