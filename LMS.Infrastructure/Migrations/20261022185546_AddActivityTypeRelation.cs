using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityTypeRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Activities_ActivityTypeId",
                table: "Activities",
                column: "ActivityTypeId");

			// Make the migration safe for databases that already have activities:
			// activities pointing to a type that does not exist get the type "Other",
			// so adding the foreign key below cannot fail.
			migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM ActivityTypes WHERE Name = 'Other')
                    INSERT INTO ActivityTypes (Name) VALUES ('Other');

                UPDATE Activities
                SET ActivityTypeId = (SELECT TOP 1 Id FROM ActivityTypes WHERE Name = 'Other')
                WHERE NOT EXISTS (
                    SELECT 1 FROM ActivityTypes t WHERE t.Id = Activities.ActivityTypeId);
                """);

			migrationBuilder.AddForeignKey(
                name: "FK_Activities_ActivityTypes_ActivityTypeId",
                table: "Activities",
                column: "ActivityTypeId",
                principalTable: "ActivityTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_ActivityTypes_ActivityTypeId",
                table: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_Activities_ActivityTypeId",
                table: "Activities");
        }
    }
}
