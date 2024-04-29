using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeFileDataIntoFilePath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileData",
                table: "Assignments");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "Assignments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "Assignments");

            migrationBuilder.AddColumn<byte[]>(
                name: "FileData",
                table: "Assignments",
                type: "bytea",
                nullable: true);
        }
    }
}
