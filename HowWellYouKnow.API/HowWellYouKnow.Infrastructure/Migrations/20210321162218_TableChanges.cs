using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HowWellYouKnow.Infrastructure.Migrations
{
    public partial class TableChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GameStateId",
                table: "Questions",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CurrentQuestionId",
                table: "GameState",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Games",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Name",
                table: "Users",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameState_CurrentQuestionId",
                table: "GameState",
                column: "CurrentQuestionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GameState_Questions_CurrentQuestionId",
                table: "GameState",
                column: "CurrentQuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameState_Questions_CurrentQuestionId",
                table: "GameState");

            migrationBuilder.DropIndex(
                name: "IX_Users_Name",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_GameState_CurrentQuestionId",
                table: "GameState");

            migrationBuilder.DropColumn(
                name: "GameStateId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "CurrentQuestionId",
                table: "GameState");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Games");
        }
    }
}
