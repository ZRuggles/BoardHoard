using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoardHoard.Migrations
{
    /// <inheritdoc />
    public partial class sync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cohort",
                table: "UserProfiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cohort",
                table: "UserProfiles",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
