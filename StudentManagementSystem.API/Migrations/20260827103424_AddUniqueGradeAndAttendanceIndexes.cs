using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentManagementSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueGradeAndAttendanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Grades_StudentId",
                table: "Grades");

            migrationBuilder.DropIndex(
                name: "IX_Attendances_StudentId",
                table: "Attendances");

            migrationBuilder.CreateIndex(
                name: "IX_Grades_StudentId_CourseId",
                table: "Grades",
                columns: new[] { "StudentId", "CourseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_StudentId_CourseId_Date",
                table: "Attendances",
                columns: new[] { "StudentId", "CourseId", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Grades_StudentId_CourseId",
                table: "Grades");

            migrationBuilder.DropIndex(
                name: "IX_Attendances_StudentId_CourseId_Date",
                table: "Attendances");

            migrationBuilder.CreateIndex(
                name: "IX_Grades_StudentId",
                table: "Grades",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_StudentId",
                table: "Attendances",
                column: "StudentId");
        }
    }
}
