using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Betting.Data.Migrations
{
    public partial class fixProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLive",
                table: "Odds");

            migrationBuilder.AddColumn<string>(
                name: "SpecialBetValue",
                table: "Odds",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "Odds",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SpecialBetValue",
                table: "Odds");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "Odds");

            migrationBuilder.AddColumn<bool>(
                name: "IsLive",
                table: "Odds",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
