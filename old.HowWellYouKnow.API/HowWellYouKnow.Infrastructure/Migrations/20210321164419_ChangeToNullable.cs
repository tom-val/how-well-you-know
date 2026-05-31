using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HowWellYouKnow.Infrastructure.Migrations
{
    public partial class ChangeToNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameState_Questions_CurrentQuestionId",
                table: "GameState");

            migrationBuilder.AlterColumn<Guid>(
                name: "CurrentQuestionId",
                table: "GameState",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddForeignKey(
                name: "FK_GameState_Questions_CurrentQuestionId",
                table: "GameState",
                column: "CurrentQuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameState_Questions_CurrentQuestionId",
                table: "GameState");

            migrationBuilder.AlterColumn<Guid>(
                name: "CurrentQuestionId",
                table: "GameState",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GameState_Questions_CurrentQuestionId",
                table: "GameState",
                column: "CurrentQuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
