using Microsoft.EntityFrameworkCore.Migrations;

namespace HowWellYouKnow.Infrastructure.Migrations
{
    public partial class UpdateNames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answer_Questions_QuestionId",
                table: "Answer");

            migrationBuilder.DropForeignKey(
                name: "FK_Answer_Users_UserId",
                table: "Answer");

            migrationBuilder.DropForeignKey(
                name: "FK_AnswerQuestionVariant_Answer_AnswersId",
                table: "AnswerQuestionVariant");

            migrationBuilder.DropForeignKey(
                name: "FK_Guess_Questions_QuestionId",
                table: "Guess");

            migrationBuilder.DropForeignKey(
                name: "FK_Guess_Users_GuessUserId",
                table: "Guess");

            migrationBuilder.DropForeignKey(
                name: "FK_Guess_Users_UserId",
                table: "Guess");

            migrationBuilder.DropForeignKey(
                name: "FK_GuessQuestionVariant_Guess_GuessesId",
                table: "GuessQuestionVariant");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestionVariantUserAnswerResult_UserAnswerResult_AnswerResultsId",
                table: "QuestionVariantUserAnswerResult");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswerResult_Questions_QuestionId",
                table: "UserAnswerResult");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswerResult_Users_UserId",
                table: "UserAnswerResult");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserAnswerResult",
                table: "UserAnswerResult");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Guess",
                table: "Guess");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Answer",
                table: "Answer");

            migrationBuilder.RenameTable(
                name: "UserAnswerResult",
                newName: "UserAnswerResults");

            migrationBuilder.RenameTable(
                name: "Guess",
                newName: "Guesses");

            migrationBuilder.RenameTable(
                name: "Answer",
                newName: "Answers");

            migrationBuilder.RenameIndex(
                name: "IX_UserAnswerResult_UserId",
                table: "UserAnswerResults",
                newName: "IX_UserAnswerResults_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserAnswerResult_QuestionId",
                table: "UserAnswerResults",
                newName: "IX_UserAnswerResults_QuestionId");

            migrationBuilder.RenameIndex(
                name: "IX_Guess_UserId",
                table: "Guesses",
                newName: "IX_Guesses_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Guess_QuestionId",
                table: "Guesses",
                newName: "IX_Guesses_QuestionId");

            migrationBuilder.RenameIndex(
                name: "IX_Guess_GuessUserId",
                table: "Guesses",
                newName: "IX_Guesses_GuessUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Answer_UserId",
                table: "Answers",
                newName: "IX_Answers_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Answer_QuestionId",
                table: "Answers",
                newName: "IX_Answers_QuestionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserAnswerResults",
                table: "UserAnswerResults",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Guesses",
                table: "Guesses",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Answers",
                table: "Answers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AnswerQuestionVariant_Answers_AnswersId",
                table: "AnswerQuestionVariant",
                column: "AnswersId",
                principalTable: "Answers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_Questions_QuestionId",
                table: "Answers",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_Users_UserId",
                table: "Answers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Guesses_Questions_QuestionId",
                table: "Guesses",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Guesses_Users_GuessUserId",
                table: "Guesses",
                column: "GuessUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Guesses_Users_UserId",
                table: "Guesses",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GuessQuestionVariant_Guesses_GuessesId",
                table: "GuessQuestionVariant",
                column: "GuessesId",
                principalTable: "Guesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionVariantUserAnswerResult_UserAnswerResults_AnswerResultsId",
                table: "QuestionVariantUserAnswerResult",
                column: "AnswerResultsId",
                principalTable: "UserAnswerResults",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswerResults_Questions_QuestionId",
                table: "UserAnswerResults",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswerResults_Users_UserId",
                table: "UserAnswerResults",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnswerQuestionVariant_Answers_AnswersId",
                table: "AnswerQuestionVariant");

            migrationBuilder.DropForeignKey(
                name: "FK_Answers_Questions_QuestionId",
                table: "Answers");

            migrationBuilder.DropForeignKey(
                name: "FK_Answers_Users_UserId",
                table: "Answers");

            migrationBuilder.DropForeignKey(
                name: "FK_Guesses_Questions_QuestionId",
                table: "Guesses");

            migrationBuilder.DropForeignKey(
                name: "FK_Guesses_Users_GuessUserId",
                table: "Guesses");

            migrationBuilder.DropForeignKey(
                name: "FK_Guesses_Users_UserId",
                table: "Guesses");

            migrationBuilder.DropForeignKey(
                name: "FK_GuessQuestionVariant_Guesses_GuessesId",
                table: "GuessQuestionVariant");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestionVariantUserAnswerResult_UserAnswerResults_AnswerResultsId",
                table: "QuestionVariantUserAnswerResult");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswerResults_Questions_QuestionId",
                table: "UserAnswerResults");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswerResults_Users_UserId",
                table: "UserAnswerResults");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserAnswerResults",
                table: "UserAnswerResults");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Guesses",
                table: "Guesses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Answers",
                table: "Answers");

            migrationBuilder.RenameTable(
                name: "UserAnswerResults",
                newName: "UserAnswerResult");

            migrationBuilder.RenameTable(
                name: "Guesses",
                newName: "Guess");

            migrationBuilder.RenameTable(
                name: "Answers",
                newName: "Answer");

            migrationBuilder.RenameIndex(
                name: "IX_UserAnswerResults_UserId",
                table: "UserAnswerResult",
                newName: "IX_UserAnswerResult_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserAnswerResults_QuestionId",
                table: "UserAnswerResult",
                newName: "IX_UserAnswerResult_QuestionId");

            migrationBuilder.RenameIndex(
                name: "IX_Guesses_UserId",
                table: "Guess",
                newName: "IX_Guess_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Guesses_QuestionId",
                table: "Guess",
                newName: "IX_Guess_QuestionId");

            migrationBuilder.RenameIndex(
                name: "IX_Guesses_GuessUserId",
                table: "Guess",
                newName: "IX_Guess_GuessUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Answers_UserId",
                table: "Answer",
                newName: "IX_Answer_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Answers_QuestionId",
                table: "Answer",
                newName: "IX_Answer_QuestionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserAnswerResult",
                table: "UserAnswerResult",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Guess",
                table: "Guess",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Answer",
                table: "Answer",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Answer_Questions_QuestionId",
                table: "Answer",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Answer_Users_UserId",
                table: "Answer",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AnswerQuestionVariant_Answer_AnswersId",
                table: "AnswerQuestionVariant",
                column: "AnswersId",
                principalTable: "Answer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Guess_Questions_QuestionId",
                table: "Guess",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Guess_Users_GuessUserId",
                table: "Guess",
                column: "GuessUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Guess_Users_UserId",
                table: "Guess",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GuessQuestionVariant_Guess_GuessesId",
                table: "GuessQuestionVariant",
                column: "GuessesId",
                principalTable: "Guess",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionVariantUserAnswerResult_UserAnswerResult_AnswerResultsId",
                table: "QuestionVariantUserAnswerResult",
                column: "AnswerResultsId",
                principalTable: "UserAnswerResult",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswerResult_Questions_QuestionId",
                table: "UserAnswerResult",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswerResult_Users_UserId",
                table: "UserAnswerResult",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
