using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lawllit.Data.Finance.Migrations
{
    public partial class AddThemeAndFontSize : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Theme",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "dark");

            migrationBuilder.AddColumn<string>(
                name: "FontSize",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "normal");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Theme",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FontSize",
                table: "Users");
        }
    }
}
