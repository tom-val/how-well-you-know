using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HowWellYouKnow.Infrastructure.Migrations
{
    public partial class AddGuessUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GuessUserId",
                table: "UserAnswerResults",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswerResults_GuessUserId",
                table: "UserAnswerResults",
                column: "GuessUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswerResults_Users_GuessUserId",
                table: "UserAnswerResults",
                column: "GuessUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswerResults_Users_GuessUserId",
                table: "UserAnswerResults");

            migrationBuilder.DropIndex(
                name: "IX_UserAnswerResults_GuessUserId",
                table: "UserAnswerResults");

            migrationBuilder.DropColumn(
                name: "GuessUserId",
                table: "UserAnswerResults");
        }
    }
}
