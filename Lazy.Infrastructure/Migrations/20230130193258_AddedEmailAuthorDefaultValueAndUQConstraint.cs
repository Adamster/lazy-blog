using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lazy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedEmailAuthorDefaultValueAndUQConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Authors",
                type: "nvarchar(max)",
                nullable: false,
                computedColumnSql: "[Name] + '@notlazy.blog'");

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: new Guid("4ea6e9c8-3bfe-450a-ab21-f118a0bfb317"),
                column: "CreatedAt",
                value: new DateTimeOffset(new DateTime(2023, 1, 30, 21, 32, 58, 36, DateTimeKind.Unspecified).AddTicks(2145), new TimeSpan(0, 2, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Authors");

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: new Guid("4ea6e9c8-3bfe-450a-ab21-f118a0bfb317"),
                column: "CreatedAt",
                value: new DateTimeOffset(new DateTime(2023, 1, 18, 11, 52, 12, 461, DateTimeKind.Unspecified).AddTicks(4093), new TimeSpan(0, 2, 0, 0, 0)));
        }
    }
}
