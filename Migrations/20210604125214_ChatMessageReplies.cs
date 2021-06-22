using Microsoft.EntityFrameworkCore.Migrations;

namespace GaryPortalAPI.Migrations
{
    public partial class ChatMessageReplies : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReplyingToUUID",
                table: "ChatMessages",
                type: "varchar(255)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ReplyingToUUID",
                table: "ChatMessages",
                column: "ReplyingToUUID");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_ChatMessages_ReplyingToUUID",
                table: "ChatMessages",
                column: "ReplyingToUUID",
                principalTable: "ChatMessages",
                principalColumn: "ChatMessageUUID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_ChatMessages_ReplyingToUUID",
                table: "ChatMessages");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_ReplyingToUUID",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "ReplyingToUUID",
                table: "ChatMessages");
        }
    }
}
