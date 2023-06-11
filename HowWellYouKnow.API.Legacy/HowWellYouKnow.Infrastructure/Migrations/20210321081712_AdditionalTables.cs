using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HowWellYouKnow.Infrastructure.Migrations
{
    public partial class AdditionalTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "QuestionVariant",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<char>(
                name: "Notation",
                table: "QuestionVariant",
                type: "TEXT",
                nullable: false,
                defaultValue: 'A');

            migrationBuilder.AddColumn<Guid>(
                name: "GameId",
                table: "Questions",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "MultipleAnswers",
                table: "Questions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "GameState",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CurrentGameState = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameState", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Answer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    QuestionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Answer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Answer_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Answer_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Game",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GameStateId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Game", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Game_GameState_GameStateId",
                        column: x => x.GameStateId,
                        principalTable: "GameState",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Game_User_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Guess",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    QuestionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GuessUserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guess", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Guess_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Guess_User_GuessUserId",
                        column: x => x.GuessUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Guess_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAnswerResult",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    QuestionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Correct = table.Column<bool>(type: "INTEGER", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAnswerResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAnswerResult_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAnswerResult_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserGameScore",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CurrentScore = table.Column<int>(type: "INTEGER", nullable: false),
                    GameStateId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGameScore", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserGameScore_GameState_GameStateId",
                        column: x => x.GameStateId,
                        principalTable: "GameState",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserGameScore_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnswerQuestionVariant",
                columns: table => new
                {
                    AnswersId = table.Column<Guid>(type: "TEXT", nullable: false),
                    QuestionVariantsId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnswerQuestionVariant", x => new { x.AnswersId, x.QuestionVariantsId });
                    table.ForeignKey(
                        name: "FK_AnswerQuestionVariant_Answer_AnswersId",
                        column: x => x.AnswersId,
                        principalTable: "Answer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnswerQuestionVariant_QuestionVariant_QuestionVariantsId",
                        column: x => x.QuestionVariantsId,
                        principalTable: "QuestionVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameUser",
                columns: table => new
                {
                    JoinedGamesId = table.Column<Guid>(type: "TEXT", nullable: false),
                    JoinedUsersId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameUser", x => new { x.JoinedGamesId, x.JoinedUsersId });
                    table.ForeignKey(
                        name: "FK_GameUser_Game_JoinedGamesId",
                        column: x => x.JoinedGamesId,
                        principalTable: "Game",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameUser_User_JoinedUsersId",
                        column: x => x.JoinedUsersId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuessQuestionVariant",
                columns: table => new
                {
                    GuessesId = table.Column<Guid>(type: "TEXT", nullable: false),
                    QuestionVariantsId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuessQuestionVariant", x => new { x.GuessesId, x.QuestionVariantsId });
                    table.ForeignKey(
                        name: "FK_GuessQuestionVariant_Guess_GuessesId",
                        column: x => x.GuessesId,
                        principalTable: "Guess",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GuessQuestionVariant_QuestionVariant_QuestionVariantsId",
                        column: x => x.QuestionVariantsId,
                        principalTable: "QuestionVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionVariantUserAnswerResult",
                columns: table => new
                {
                    AnswerResultsId = table.Column<Guid>(type: "TEXT", nullable: false),
                    QuestionVariantsId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionVariantUserAnswerResult", x => new { x.AnswerResultsId, x.QuestionVariantsId });
                    table.ForeignKey(
                        name: "FK_QuestionVariantUserAnswerResult_QuestionVariant_QuestionVariantsId",
                        column: x => x.QuestionVariantsId,
                        principalTable: "QuestionVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestionVariantUserAnswerResult_UserAnswerResult_AnswerResultsId",
                        column: x => x.AnswerResultsId,
                        principalTable: "UserAnswerResult",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Questions_GameId",
                table: "Questions",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Answer_QuestionId",
                table: "Answer",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_Answer_UserId",
                table: "Answer",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AnswerQuestionVariant_QuestionVariantsId",
                table: "AnswerQuestionVariant",
                column: "QuestionVariantsId");

            migrationBuilder.CreateIndex(
                name: "IX_Game_CreatedByUserId",
                table: "Game",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Game_GameStateId",
                table: "Game",
                column: "GameStateId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameUser_JoinedUsersId",
                table: "GameUser",
                column: "JoinedUsersId");

            migrationBuilder.CreateIndex(
                name: "IX_Guess_GuessUserId",
                table: "Guess",
                column: "GuessUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Guess_QuestionId",
                table: "Guess",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_Guess_UserId",
                table: "Guess",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GuessQuestionVariant_QuestionVariantsId",
                table: "GuessQuestionVariant",
                column: "QuestionVariantsId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionVariantUserAnswerResult_QuestionVariantsId",
                table: "QuestionVariantUserAnswerResult",
                column: "QuestionVariantsId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswerResult_QuestionId",
                table: "UserAnswerResult",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswerResult_UserId",
                table: "UserAnswerResult",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGameScore_GameStateId",
                table: "UserGameScore",
                column: "GameStateId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGameScore_UserId",
                table: "UserGameScore",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Game_GameId",
                table: "Questions",
                column: "GameId",
                principalTable: "Game",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Game_GameId",
                table: "Questions");

            migrationBuilder.DropTable(
                name: "AnswerQuestionVariant");

            migrationBuilder.DropTable(
                name: "GameUser");

            migrationBuilder.DropTable(
                name: "GuessQuestionVariant");

            migrationBuilder.DropTable(
                name: "QuestionVariantUserAnswerResult");

            migrationBuilder.DropTable(
                name: "UserGameScore");

            migrationBuilder.DropTable(
                name: "Answer");

            migrationBuilder.DropTable(
                name: "Game");

            migrationBuilder.DropTable(
                name: "Guess");

            migrationBuilder.DropTable(
                name: "UserAnswerResult");

            migrationBuilder.DropTable(
                name: "GameState");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropIndex(
                name: "IX_Questions_GameId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Notation",
                table: "QuestionVariant");

            migrationBuilder.DropColumn(
                name: "GameId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "MultipleAnswers",
                table: "Questions");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "QuestionVariant",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50);
        }
    }
}
