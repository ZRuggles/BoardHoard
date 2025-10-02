using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoardHoardAPI.Migrations
{
    /// <inheritdoc />
    public partial class RecreateReviewsIfMissing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'dbo.Reviews', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[Reviews](
                        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [Kind] INT NOT NULL,
                        [GameId] INT NOT NULL,
                        [UserId] NVARCHAR(450) NOT NULL,
                        [Rating] FLOAT NOT NULL,
                        [Title] NVARCHAR(MAX) NULL,
                        [Body] NVARCHAR(MAX) NULL,
                        [CreatedAt] DATETIMEOFFSET NOT NULL,
                        [UpdatedAt] DATETIMEOFFSET NULL
                    );
                    CREATE UNIQUE INDEX [IX_Reviews_UserId_Kind_GameId]
                        ON [dbo].[Reviews]([UserId], [Kind], [GameId]);
                END
                ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Only drop if exists (in case it wasn’t created by this migration)
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.Reviews', N'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[Reviews];
END
");
        }
    }
}
