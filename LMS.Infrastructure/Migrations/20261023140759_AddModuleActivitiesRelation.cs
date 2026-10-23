using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddModuleActivitiesRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ModuleId1",
                table: "Activities",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Activities_ModuleId1",
                table: "Activities",
                column: "ModuleId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_Modules_ModuleId1",
                table: "Activities",
                column: "ModuleId1",
                principalTable: "Modules",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_Modules_ModuleId1",
                table: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_Activities_ModuleId1",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "ModuleId1",
                table: "Activities");
        }
    }
}
