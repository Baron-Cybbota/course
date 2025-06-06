using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace course.Migrations
{
    /// <inheritdoc />
    public partial class DelIsEditedfrPostComm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEdited",
                table: "Posts");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastEditDate",
                table: "Comments",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastEditDate",
                table: "Comments");

            migrationBuilder.AddColumn<bool>(
                name: "IsEdited",
                table: "Posts",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
