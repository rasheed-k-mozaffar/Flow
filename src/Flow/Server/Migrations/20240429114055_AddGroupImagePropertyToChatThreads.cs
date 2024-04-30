using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flow.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupImagePropertyToChatThreads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ChatThreadId",
                table: "Images",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Images_ChatThreadId",
                table: "Images",
                column: "ChatThreadId",
                unique: true,
                filter: "[ChatThreadId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Threads_ChatThreadId",
                table: "Images",
                column: "ChatThreadId",
                principalTable: "Threads",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_Threads_ChatThreadId",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Images_ChatThreadId",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "ChatThreadId",
                table: "Images");
        }
    }
}
