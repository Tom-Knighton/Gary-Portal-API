using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GaryPortalAPI.Migrations
{
    public partial class EventsCommandments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Commandments",
                columns: table => new
                {
                    CommandmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CommandmentName = table.Column<string>(type: "longtext", nullable: true),
                    CommandmentDescription = table.Column<string>(type: "longtext", nullable: true),
                    CommandmentCoverUrl = table.Column<string>(type: "longtext", nullable: true),
                    CommandmentIsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commandments", x => x.CommandmentId);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    EventId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EventName = table.Column<string>(type: "longtext", nullable: true),
                    EventDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EventEndsAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EventShortDescription = table.Column<string>(type: "longtext", nullable: true),
                    EventDescription = table.Column<string>(type: "longtext", nullable: true),
                    EventTeamId = table.Column<int>(type: "int", nullable: false),
                    IsEventDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.EventId);
                    table.ForeignKey(
                        name: "FK_Events_Teams_EventTeamId",
                        column: x => x.EventTeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_EventTeamId",
                table: "Events",
                column: "EventTeamId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Commandments");

            migrationBuilder.DropTable(
                name: "Events");
        }
    }
}
