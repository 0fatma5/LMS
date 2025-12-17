using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Migrations
{
    /// <inheritdoc />
    public partial class AddModulesAndRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Modules_Courses_CourseId",
                table: "Modules");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DeleteData(
                table: "Lessons",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.RenameColumn(
                name: "CourseId",
                table: "Modules",
                newName: "SubjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Modules_CourseId",
                table: "Modules",
                newName: "IX_Modules_SubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Modules_Subjects_SubjectId",
                table: "Modules",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Modules_Subjects_SubjectId",
                table: "Modules");

            migrationBuilder.RenameColumn(
                name: "SubjectId",
                table: "Modules",
                newName: "CourseId");

            migrationBuilder.RenameIndex(
                name: "IX_Modules_SubjectId",
                table: "Modules",
                newName: "IX_Modules_CourseId");

            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoverImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Courses",
                columns: new[] { "Id", "CoverImageUrl", "Description", "Title" },
                values: new object[] { 1, "https://via.placeholder.com/300", "Learn generic web development from scratch using .NET and React.", "Full Stack Web Development" });

            migrationBuilder.InsertData(
                table: "Lessons",
                columns: new[] { "Id", "ContentUrl", "DurationMinutes", "IsPreview", "ModuleId", "TextContent", "Title", "Type" },
                values: new object[] { 3, "", 10, false, 1, "<h1>Understanding .NET</h1>", "Why .NET?", 2 });

            migrationBuilder.AddForeignKey(
                name: "FK_Modules_Courses_CourseId",
                table: "Modules",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
