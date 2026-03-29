using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RotinaXP.API.Migrations
{
    /// <inheritdoc />
    public partial class AddHighLoadIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "DailyProgresses",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateIndex(
                name: "IX_DailyProgresses_UserId",
                table: "DailyProgresses",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DailyProgresses_UserId",
                table: "DailyProgresses");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "DailyProgresses",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");
        }
    }
}
