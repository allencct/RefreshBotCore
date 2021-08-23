using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RefreshWeb.Migrations
{
    public partial class TargetPageContent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Content",
                table: "TargetPages",
                type: "bytea",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "TargetPages");
        }
    }
}
