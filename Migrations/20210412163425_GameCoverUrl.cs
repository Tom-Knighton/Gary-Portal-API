using Microsoft.EntityFrameworkCore.Migrations;

namespace GaryPortalAPI.Migrations
{
    public partial class GameCoverUrl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GameCoverUrl",
                table: "GameTypes",
                type: "longtext",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GameCoverUrl",
                table: "GameTypes");
        }
    }
}
