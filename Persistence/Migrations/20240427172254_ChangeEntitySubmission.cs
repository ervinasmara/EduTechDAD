using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeEntitySubmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileData",
                table: "AssignmentSubmissions");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SubmissionTime",
                table: "AssignmentSubmissions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "Grade",
                table: "AssignmentSubmissions",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "AssignmentSubmissions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "AssignmentSubmissions");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SubmissionTime",
                table: "AssignmentSubmissions",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<float>(
                name: "Grade",
                table: "AssignmentSubmissions",
                type: "real",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AddColumn<byte[]>(
                name: "FileData",
                table: "AssignmentSubmissions",
                type: "bytea",
                nullable: true);
        }
    }
}
