using Microsoft.EntityFrameworkCore.Migrations;

namespace Spice.Data.Migrations
{
    public partial class SpellMistakeCorrected : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Spiceness",
                table: "MenuItems");

            migrationBuilder.AddColumn<string>(
                name: "Spicyness",
                table: "MenuItems",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Spicyness",
                table: "MenuItems");

            migrationBuilder.AddColumn<string>(
                name: "Spiceness",
                table: "MenuItems",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
