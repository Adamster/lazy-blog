using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lazy.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedPostViewCounter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Views",
                table: "Posts",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Views",
                table: "Posts");
        }
    }
}
