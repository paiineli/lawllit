using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lawllit.Data.Finance.Migrations
{
    public partial class AddIsOnboardingCompleted : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOnboardingCompleted",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOnboardingCompleted",
                table: "Users");
        }
    }
}
