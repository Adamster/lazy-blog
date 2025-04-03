using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lazy.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PopulatePublishDateWithCreatedDateForPosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            UPDATE Posts
            SET PublishedOnUtc = CreatedOnUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
