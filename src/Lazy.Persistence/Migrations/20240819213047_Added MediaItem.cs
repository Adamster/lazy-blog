using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lazy.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedMediaItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageMediaItem_Url",
                table: "Users",
                newName: "Avatar_Url");

            migrationBuilder.RenameColumn(
                name: "ImageMediaItem_Filename",
                table: "Users",
                newName: "Avatar_Filename");

            migrationBuilder.CreateTable(
                name: "MediaItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UploadedUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediaItems_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MediaItems_UserId",
                table: "MediaItems",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MediaItems");

            migrationBuilder.RenameColumn(
                name: "Avatar_Url",
                table: "Users",
                newName: "ImageMediaItem_Url");

            migrationBuilder.RenameColumn(
                name: "Avatar_Filename",
                table: "Users",
                newName: "ImageMediaItem_Filename");
        }
    }
}
