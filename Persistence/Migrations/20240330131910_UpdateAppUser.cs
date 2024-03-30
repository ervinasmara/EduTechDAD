using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAppUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SuperAdmins_AppUserId",
                table: "SuperAdmins");

            migrationBuilder.CreateIndex(
                name: "IX_SuperAdmins_AppUserId",
                table: "SuperAdmins",
                column: "AppUserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SuperAdmins_AppUserId",
                table: "SuperAdmins");

            migrationBuilder.CreateIndex(
                name: "IX_SuperAdmins_AppUserId",
                table: "SuperAdmins",
                column: "AppUserId");
        }
    }
}
