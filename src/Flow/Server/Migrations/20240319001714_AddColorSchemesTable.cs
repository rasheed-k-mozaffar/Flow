using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flow.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddColorSchemesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccentsColor",
                table: "SettingsEntries");

            migrationBuilder.DropColumn(
                name: "MessageBubbleColor",
                table: "SettingsEntries");

            migrationBuilder.DropColumn(
                name: "SendButtonColor",
                table: "SettingsEntries");

            migrationBuilder.AddColumn<int>(
                name: "ColorSchemeId",
                table: "SettingsEntries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ColorSchemes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    SentMsgBubbleColor = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ReceivedMsgBubbleColor = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    AccentsColor = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    SelectedMessageColor = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColorSchemes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ColorSchemes",
                columns: new[] { "Id", "AccentsColor", "Name", "ReceivedMsgBubbleColor", "SelectedMessageColor", "SentMsgBubbleColor" },
                values: new object[] { 1, "text-blue-500 bg-blue-600", "Flow's Default", "bg-gray-100 text-gray-600", "bg-red-500 text-white", "bg-blue-600 text-white" });

            migrationBuilder.CreateIndex(
                name: "IX_SettingsEntries_ColorSchemeId",
                table: "SettingsEntries",
                column: "ColorSchemeId");

            migrationBuilder.AddForeignKey(
                name: "FK_SettingsEntries_ColorSchemes_ColorSchemeId",
                table: "SettingsEntries",
                column: "ColorSchemeId",
                principalTable: "ColorSchemes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SettingsEntries_ColorSchemes_ColorSchemeId",
                table: "SettingsEntries");

            migrationBuilder.DropTable(
                name: "ColorSchemes");

            migrationBuilder.DropIndex(
                name: "IX_SettingsEntries_ColorSchemeId",
                table: "SettingsEntries");

            migrationBuilder.DropColumn(
                name: "ColorSchemeId",
                table: "SettingsEntries");

            migrationBuilder.AddColumn<string>(
                name: "AccentsColor",
                table: "SettingsEntries",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MessageBubbleColor",
                table: "SettingsEntries",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SendButtonColor",
                table: "SettingsEntries",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");
        }
    }
}
