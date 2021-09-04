using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GaryPortalAPI.Migrations
{
    public partial class UserFlags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsQueued",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NotificationsMuted",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserIsAdmin",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserIsStaff",
                table: "Users");

            migrationBuilder.CreateTable(
                name: "Flags",
                columns: table => new
                {
                    FlagId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FlagName = table.Column<string>(type: "longtext", nullable: true),
                    FlagAccessLevel = table.Column<int>(type: "int", nullable: false),
                    FlagIsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flags", x => x.FlagId);
                });

            migrationBuilder.CreateTable(
                name: "UserFlags",
                columns: table => new
                {
                    UserUUID = table.Column<string>(type: "varchar(255)", nullable: false),
                    FlagId = table.Column<int>(type: "int", nullable: false),
                    FlagEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFlags", x => new { x.FlagId, x.UserUUID });
                    table.ForeignKey(
                        name: "FK_UserFlags_Flags_FlagId",
                        column: x => x.FlagId,
                        principalTable: "Flags",
                        principalColumn: "FlagId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFlags_Users_UserUUID",
                        column: x => x.UserUUID,
                        principalTable: "Users",
                        principalColumn: "UserUUID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserFlags_UserUUID",
                table: "UserFlags",
                column: "UserUUID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserFlags");

            migrationBuilder.DropTable(
                name: "Flags");

            migrationBuilder.AddColumn<bool>(
                name: "IsQueued",
                table: "Users",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotificationsMuted",
                table: "Users",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UserIsAdmin",
                table: "Users",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UserIsStaff",
                table: "Users",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
