using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlignDocumentWithErDiagram : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Documents_StorageKey",
                table: "Documents");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Documents_OneTarget",
                table: "Documents");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Documents_Size",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "StorageKey",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "UploadedByName",
                table: "Documents");

            migrationBuilder.AddColumn<string>(
                name: "BlobPath",
                table: "Documents",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Documents",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Documents",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_BlobPath",
                table: "Documents",
                column: "BlobPath",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Description",
                table: "Documents",
                column: "Description",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Name",
                table: "Documents",
                column: "Name",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Documents_OneTarget",
                table: "Documents",
                sql: "[SizeBytes] > 0 AND [SizeBytes] <= 10458760");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Documents_BlobPath",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_Description",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_Name",
                table: "Documents");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Documents_OneTarget",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "BlobPath",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Documents");

            migrationBuilder.AddColumn<string>(
                name: "StorageKey",
                table: "Documents",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UploadedByName",
                table: "Documents",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_StorageKey",
                table: "Documents",
                column: "StorageKey",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Documents_OneTarget",
                table: "Documents",
                sql: "(\r\n    CASE WHEN [CourseId] IS NULL THEN 0 ELSE 1 END +\r\n    CASE WHEN [ModuleId] IS NULL THEN 0 ELSE 1 END +\r\n    CASE WHEN [ActivityId] IS NULL THEN 0 ELSE 1 END\r\n) = 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Documents_Size",
                table: "Documents",
                sql: "[SizeBytes] > 0 AND [SizeBytes] <= 10485760");
        }
    }
}
