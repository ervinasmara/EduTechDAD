using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNameUniqueNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UniqueNumber",
                table: "Lessons",
                newName: "UniqueNumberOfLesson");

            migrationBuilder.RenameColumn(
                name: "UniqueNumber",
                table: "ClassRooms",
                newName: "UniqueNumberOfClassRoom");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UniqueNumberOfLesson",
                table: "Lessons",
                newName: "UniqueNumber");

            migrationBuilder.RenameColumn(
                name: "UniqueNumberOfClassRoom",
                table: "ClassRooms",
                newName: "UniqueNumber");
        }
    }
}
