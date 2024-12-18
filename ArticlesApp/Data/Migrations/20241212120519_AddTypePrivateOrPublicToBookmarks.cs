﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pinterest.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTypePrivateOrPublicToBookmarks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Bookmarks",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Bookmarks");
        }
    }
}
