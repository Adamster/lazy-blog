using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lazy.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAvatarToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Avatar_Filename",
                table: "Users",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Avatar_Url",
                table: "Users",
                type: "nvarchar(600)",
                maxLength: 600,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Avatar_Filename",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Avatar_Url",
                table: "Users");
        }
    }
}
