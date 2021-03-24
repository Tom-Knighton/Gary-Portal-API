using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GaryPortalAPI.Migrations
{
    public partial class stickers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Stickers",
                columns: table => new
                {
                    StickerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    StickerURL = table.Column<string>(type: "longtext", nullable: true),
                    StickerName = table.Column<string>(type: "longtext", nullable: true),
                    StickerStaffOnly = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    StickerAdminOnly = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    StickerIsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stickers", x => x.StickerId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Stickers");
        }
    }
}
