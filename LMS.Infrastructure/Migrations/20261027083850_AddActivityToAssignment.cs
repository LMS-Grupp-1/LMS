using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityToAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			// Step 1: add the column as nullable so existing rows do not break the migration.
			migrationBuilder.AddColumn<int>(
				name: "ActivityId",
				table: "Assignments",
				type: "int",
				nullable: true);

			// Step 2: modules that have assignments but no activities get a placeholder activity,
			// so every existing assignment has something to belong to.
			migrationBuilder.Sql("""
                INSERT INTO Activities (Name, Description, StartDate, EndDate, ModuleId, ActivityTypeId)
                SELECT N'Uppgifter',
                       N'Skapad automatiskt för befintliga uppgifter.',
                       m.StartDate,
                       DATEADD(HOUR, 1, m.StartDate),
                       m.Id,
                       (SELECT TOP 1 Id FROM ActivityTypes WHERE Name = N'Other')
                FROM Modules m
                WHERE EXISTS (SELECT 1 FROM Assignments a WHERE a.ModuleId = m.Id)
                  AND NOT EXISTS (SELECT 1 FROM Activities ac WHERE ac.ModuleId = m.Id);
                """);

			// Step 3: link each existing assignment to the first activity in its module.
			migrationBuilder.Sql("""
                UPDATE a
                SET ActivityId = (
                    SELECT TOP 1 ac.Id
                    FROM Activities ac
                    WHERE ac.ModuleId = a.ModuleId
                    ORDER BY ac.StartDate)
                FROM Assignments a
                WHERE a.ActivityId IS NULL;
                """);

			// Step 4: every row now has an activity, so the column can be required.
			migrationBuilder.AlterColumn<int>(
				name: "ActivityId",
				table: "Assignments",
				type: "int",
				nullable: false,
				oldClrType: typeof(int),
				oldType: "int",
				oldNullable: true);

			migrationBuilder.CreateIndex(
                name: "IX_Assignments_ActivityId",
                table: "Assignments",
                column: "ActivityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_Activities_ActivityId",
                table: "Assignments",
                column: "ActivityId",
                principalTable: "Activities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_Activities_ActivityId",
                table: "Assignments");

            migrationBuilder.DropIndex(
                name: "IX_Assignments_ActivityId",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "ActivityId",
                table: "Assignments");
        }
    }
}
