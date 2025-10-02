using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoardHoardAPI.Migrations
{
    /// <inheritdoc />
    public partial class @float : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.Reviews', 'Rating') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[Reviews] ALTER COLUMN [Rating] float NOT NULL;
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
