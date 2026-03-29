using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RotinaXP.API.Migrations
{
    /// <inheritdoc />
    public partial class AddConcurrencyAndDailyProgressUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DailyProgresses_UserId",
                table: "DailyProgresses");

            migrationBuilder.AddColumn<long>(
                name: "RowVersion",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_DailyProgresses_UserId_Date",
                table: "DailyProgresses",
                columns: new[] { "UserId", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DailyProgresses_UserId_Date",
                table: "DailyProgresses");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_DailyProgresses_UserId",
                table: "DailyProgresses",
                column: "UserId");
        }
    }
}
