using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HowWellYouKnow.Infrastructure.Migrations
{
    public partial class UpdateGuess : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuestionVariantUserAnswerResult_QuestionVariant_QuestionVariantsId",
                table: "QuestionVariantUserAnswerResult");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QuestionVariantUserAnswerResult",
                table: "QuestionVariantUserAnswerResult");

            migrationBuilder.DropIndex(
                name: "IX_QuestionVariantUserAnswerResult_QuestionVariantsId",
                table: "QuestionVariantUserAnswerResult");

            migrationBuilder.RenameColumn(
                name: "QuestionVariantsId",
                table: "QuestionVariantUserAnswerResult",
                newName: "AnswerQuestionVariantsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuestionVariantUserAnswerResult",
                table: "QuestionVariantUserAnswerResult",
                columns: new[] { "AnswerQuestionVariantsId", "AnswerResultsId" });

            migrationBuilder.CreateTable(
                name: "QuestionVariantUserAnswerResult1",
                columns: table => new
                {
                    GuessQuestionVariantsId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GuessResultsId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionVariantUserAnswerResult1", x => new { x.GuessQuestionVariantsId, x.GuessResultsId });
                    table.ForeignKey(
                        name: "FK_QuestionVariantUserAnswerResult1_QuestionVariant_GuessQuestionVariantsId",
                        column: x => x.GuessQuestionVariantsId,
                        principalTable: "QuestionVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestionVariantUserAnswerResult1_UserAnswerResults_GuessResultsId",
                        column: x => x.GuessResultsId,
                        principalTable: "UserAnswerResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionVariantUserAnswerResult_AnswerResultsId",
                table: "QuestionVariantUserAnswerResult",
                column: "AnswerResultsId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionVariantUserAnswerResult1_GuessResultsId",
                table: "QuestionVariantUserAnswerResult1",
                column: "GuessResultsId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionVariantUserAnswerResult_QuestionVariant_AnswerQuestionVariantsId",
                table: "QuestionVariantUserAnswerResult",
                column: "AnswerQuestionVariantsId",
                principalTable: "QuestionVariant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuestionVariantUserAnswerResult_QuestionVariant_AnswerQuestionVariantsId",
                table: "QuestionVariantUserAnswerResult");

            migrationBuilder.DropTable(
                name: "QuestionVariantUserAnswerResult1");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QuestionVariantUserAnswerResult",
                table: "QuestionVariantUserAnswerResult");

            migrationBuilder.DropIndex(
                name: "IX_QuestionVariantUserAnswerResult_AnswerResultsId",
                table: "QuestionVariantUserAnswerResult");

            migrationBuilder.RenameColumn(
                name: "AnswerQuestionVariantsId",
                table: "QuestionVariantUserAnswerResult",
                newName: "QuestionVariantsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuestionVariantUserAnswerResult",
                table: "QuestionVariantUserAnswerResult",
                columns: new[] { "AnswerResultsId", "QuestionVariantsId" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionVariantUserAnswerResult_QuestionVariantsId",
                table: "QuestionVariantUserAnswerResult",
                column: "QuestionVariantsId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionVariantUserAnswerResult_QuestionVariant_QuestionVariantsId",
                table: "QuestionVariantUserAnswerResult",
                column: "QuestionVariantsId",
                principalTable: "QuestionVariant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
