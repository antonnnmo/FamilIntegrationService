using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FamilServiceMonitoringApp.Migrations
{
    public partial class ContactInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContactInfoEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Timeout = table.Column<int>(type: "int", nullable: false),
                    EventName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsError = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactInfoEvents", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactInfoEvents");
        }
    }
}
