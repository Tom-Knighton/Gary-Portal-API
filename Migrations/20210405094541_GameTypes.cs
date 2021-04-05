using Microsoft.EntityFrameworkCore.Migrations;

namespace GaryPortalAPI.Migrations
{
    public partial class GameTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameTypes",
                columns: table => new
                {
                    GameUUID = table.Column<string>(type: "varchar(255)", nullable: false),
                    GameName = table.Column<string>(type: "longtext", nullable: true),
                    GameDescription = table.Column<string>(type: "longtext", nullable: true),
                    GameTeamId = table.Column<int>(type: "int", nullable: false),
                    GameIsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameTypes", x => x.GameUUID);
                    table.ForeignKey(
                        name: "FK_GameTypes_Teams_GameTeamId",
                        column: x => x.GameTeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameTypes_GameTeamId",
                table: "GameTypes",
                column: "GameTeamId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameTypes");
        }
    }
}
