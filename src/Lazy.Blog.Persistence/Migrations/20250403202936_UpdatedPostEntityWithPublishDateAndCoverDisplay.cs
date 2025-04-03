using System;
using Lazy.Persistence.Constants;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Lazy.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedPostEntityWithPublishDateAndCoverDisplay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("8c754b8a-0f8e-4110-ada5-9d12a5b13cf1"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a37f312c-5513-40af-8907-578c0bd9b143"));

            migrationBuilder.AddColumn<bool>(
                name: "IsCoverDisplayed",
                table: "Posts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedOnUtc",
                table: "Posts",
                type: "datetime2",
                nullable: true);


            migrationBuilder.Sql(@"
        UPDATE Posts
        SET PublishedOnUtc = CreatedOnUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCoverDisplayed",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "PublishedOnUtc",
                table: "Posts");

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("8c754b8a-0f8e-4110-ada5-9d12a5b13cf1"), null, "Admin", null },
                    { new Guid("a37f312c-5513-40af-8907-578c0bd9b143"), null, "Member", null }
                });
        }
    }
}
