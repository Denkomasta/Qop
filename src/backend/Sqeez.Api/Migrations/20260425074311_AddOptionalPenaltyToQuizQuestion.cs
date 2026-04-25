using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sqeez.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddOptionalPenaltyToQuizQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Penalty",
                table: "QuizQuestions",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Penalty",
                table: "QuizQuestions");
        }
    }
}
