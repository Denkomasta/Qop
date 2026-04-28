using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sqeez.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintToQuizResponses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QuizQuestionResponses_QuizAttemptId",
                table: "QuizQuestionResponses");

            migrationBuilder.CreateIndex(
                name: "IX_QuizQuestionResponses_QuizAttemptId_QuizQuestionId",
                table: "QuizQuestionResponses",
                columns: new[] { "QuizAttemptId", "QuizQuestionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QuizQuestionResponses_QuizAttemptId_QuizQuestionId",
                table: "QuizQuestionResponses");

            migrationBuilder.CreateIndex(
                name: "IX_QuizQuestionResponses_QuizAttemptId",
                table: "QuizQuestionResponses",
                column: "QuizAttemptId");
        }
    }
}
