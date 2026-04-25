using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sqeez.Api.Migrations
{
    /// <inheritdoc />
    public partial class RefactorPenaltyQuizQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Penalty",
                table: "QuizQuestions");

            migrationBuilder.AddColumn<bool>(
                name: "HasPenalty",
                table: "QuizQuestions",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasPenalty",
                table: "QuizQuestions");

            migrationBuilder.AddColumn<int>(
                name: "Penalty",
                table: "QuizQuestions",
                type: "integer",
                nullable: true);
        }
    }
}
