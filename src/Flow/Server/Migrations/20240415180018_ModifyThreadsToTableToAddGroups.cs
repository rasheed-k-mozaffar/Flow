using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flow.Server.Migrations
{
    /// <inheritdoc />
    public partial class ModifyThreadsToTableToAddGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Threads",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Threads",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Threads",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Threads");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Threads");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Threads");
        }
    }
}
