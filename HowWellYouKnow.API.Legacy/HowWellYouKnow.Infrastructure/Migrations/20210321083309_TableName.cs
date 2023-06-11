using Microsoft.EntityFrameworkCore.Migrations;

namespace HowWellYouKnow.Infrastructure.Migrations
{
    public partial class TableName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answer_User_UserId",
                table: "Answer");

            migrationBuilder.DropForeignKey(
                name: "FK_Game_GameState_GameStateId",
                table: "Game");

            migrationBuilder.DropForeignKey(
                name: "FK_Game_User_CreatedByUserId",
                table: "Game");

            migrationBuilder.DropForeignKey(
                name: "FK_GameUser_Game_JoinedGamesId",
                table: "GameUser");

            migrationBuilder.DropForeignKey(
                name: "FK_GameUser_User_JoinedUsersId",
                table: "GameUser");

            migrationBuilder.DropForeignKey(
                name: "FK_Guess_User_GuessUserId",
                table: "Guess");

            migrationBuilder.DropForeignKey(
                name: "FK_Guess_User_UserId",
                table: "Guess");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Game_GameId",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswerResult_User_UserId",
                table: "UserAnswerResult");

            migrationBuilder.DropForeignKey(
                name: "FK_UserGameScore_User_UserId",
                table: "UserGameScore");

            migrationBuilder.DropPrimaryKey(
                name: "PK_User",
                table: "User");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Game",
                table: "Game");

            migrationBuilder.RenameTable(
                name: "User",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "Game",
                newName: "Games");

            migrationBuilder.RenameIndex(
                name: "IX_Game_GameStateId",
                table: "Games",
                newName: "IX_Games_GameStateId");

            migrationBuilder.RenameIndex(
                name: "IX_Game_CreatedByUserId",
                table: "Games",
                newName: "IX_Games_CreatedByUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Games",
                table: "Games",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Answer_Users_UserId",
                table: "Answer",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Games_GameState_GameStateId",
                table: "Games",
                column: "GameStateId",
                principalTable: "GameState",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Users_CreatedByUserId",
                table: "Games",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GameUser_Games_JoinedGamesId",
                table: "GameUser",
                column: "JoinedGamesId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GameUser_Users_JoinedUsersId",
                table: "GameUser",
                column: "JoinedUsersId",
                principalTable: "Users",
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
                name: "FK_Questions_Games_GameId",
                table: "Questions",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswerResult_Users_UserId",
                table: "UserAnswerResult",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserGameScore_Users_UserId",
                table: "UserGameScore",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answer_Users_UserId",
                table: "Answer");

            migrationBuilder.DropForeignKey(
                name: "FK_Games_GameState_GameStateId",
                table: "Games");

            migrationBuilder.DropForeignKey(
                name: "FK_Games_Users_CreatedByUserId",
                table: "Games");

            migrationBuilder.DropForeignKey(
                name: "FK_GameUser_Games_JoinedGamesId",
                table: "GameUser");

            migrationBuilder.DropForeignKey(
                name: "FK_GameUser_Users_JoinedUsersId",
                table: "GameUser");

            migrationBuilder.DropForeignKey(
                name: "FK_Guess_Users_GuessUserId",
                table: "Guess");

            migrationBuilder.DropForeignKey(
                name: "FK_Guess_Users_UserId",
                table: "Guess");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Games_GameId",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswerResult_Users_UserId",
                table: "UserAnswerResult");

            migrationBuilder.DropForeignKey(
                name: "FK_UserGameScore_Users_UserId",
                table: "UserGameScore");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Games",
                table: "Games");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "User");

            migrationBuilder.RenameTable(
                name: "Games",
                newName: "Game");

            migrationBuilder.RenameIndex(
                name: "IX_Games_GameStateId",
                table: "Game",
                newName: "IX_Game_GameStateId");

            migrationBuilder.RenameIndex(
                name: "IX_Games_CreatedByUserId",
                table: "Game",
                newName: "IX_Game_CreatedByUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_User",
                table: "User",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Game",
                table: "Game",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Answer_User_UserId",
                table: "Answer",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Game_GameState_GameStateId",
                table: "Game",
                column: "GameStateId",
                principalTable: "GameState",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Game_User_CreatedByUserId",
                table: "Game",
                column: "CreatedByUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GameUser_Game_JoinedGamesId",
                table: "GameUser",
                column: "JoinedGamesId",
                principalTable: "Game",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GameUser_User_JoinedUsersId",
                table: "GameUser",
                column: "JoinedUsersId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Guess_User_GuessUserId",
                table: "Guess",
                column: "GuessUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Guess_User_UserId",
                table: "Guess",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Game_GameId",
                table: "Questions",
                column: "GameId",
                principalTable: "Game",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswerResult_User_UserId",
                table: "UserAnswerResult",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserGameScore_User_UserId",
                table: "UserGameScore",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
