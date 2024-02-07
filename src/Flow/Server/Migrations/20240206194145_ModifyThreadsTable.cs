using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flow.Server.Migrations
{
    /// <inheritdoc />
    public partial class ModifyThreadsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Seen",
                table: "Notifications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ChatThreadId",
                table: "ContactRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactRequests_ChatThreadId",
                table: "ContactRequests",
                column: "ChatThreadId",
                unique: true,
                filter: "[ChatThreadId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ContactRequests_Threads_ChatThreadId",
                table: "ContactRequests",
                column: "ChatThreadId",
                principalTable: "Threads",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContactRequests_Threads_ChatThreadId",
                table: "ContactRequests");

            migrationBuilder.DropIndex(
                name: "IX_ContactRequests_ChatThreadId",
                table: "ContactRequests");

            migrationBuilder.DropColumn(
                name: "Seen",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ChatThreadId",
                table: "ContactRequests");
        }
    }
}
