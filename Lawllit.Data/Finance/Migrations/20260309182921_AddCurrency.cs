using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lawllit.Data.Finance.Migrations
{
    public partial class AddCurrency : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "BRL");

            migrationBuilder.Sql("UPDATE \"Users\" SET \"Currency\" = 'BRL' WHERE \"Currency\" = ''");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Users");
        }
    }
}
