using Microsoft.EntityFrameworkCore.Migrations;

namespace FamilServiceMonitoringApp.Migrations
{
    public partial class EventErrorField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsError",
                table: "Events",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsError",
                table: "Events");
        }
    }
}
