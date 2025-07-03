using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Migrations
{
    /// <inheritdoc />
    public partial class refactorTicketHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketHistoryModel_AspNetUsers_ChangedByUserId",
                table: "TicketHistoryModel");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketHistoryModel_Tickets_TicketId",
                table: "TicketHistoryModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TicketHistoryModel",
                table: "TicketHistoryModel");

            migrationBuilder.RenameTable(
                name: "TicketHistoryModel",
                newName: "TicketHistories");

            migrationBuilder.RenameIndex(
                name: "IX_TicketHistoryModel_TicketId",
                table: "TicketHistories",
                newName: "IX_TicketHistories_TicketId");

            migrationBuilder.RenameIndex(
                name: "IX_TicketHistoryModel_ChangedByUserId",
                table: "TicketHistories",
                newName: "IX_TicketHistories_ChangedByUserId");

            migrationBuilder.AlterColumn<string>(
                name: "PropertyName",
                table: "TicketHistories",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TicketHistories",
                table: "TicketHistories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketHistories_AspNetUsers_ChangedByUserId",
                table: "TicketHistories",
                column: "ChangedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketHistories_Tickets_TicketId",
                table: "TicketHistories",
                column: "TicketId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketHistories_AspNetUsers_ChangedByUserId",
                table: "TicketHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketHistories_Tickets_TicketId",
                table: "TicketHistories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TicketHistories",
                table: "TicketHistories");

            migrationBuilder.RenameTable(
                name: "TicketHistories",
                newName: "TicketHistoryModel");

            migrationBuilder.RenameIndex(
                name: "IX_TicketHistories_TicketId",
                table: "TicketHistoryModel",
                newName: "IX_TicketHistoryModel_TicketId");

            migrationBuilder.RenameIndex(
                name: "IX_TicketHistories_ChangedByUserId",
                table: "TicketHistoryModel",
                newName: "IX_TicketHistoryModel_ChangedByUserId");

            migrationBuilder.AlterColumn<string>(
                name: "PropertyName",
                table: "TicketHistoryModel",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TicketHistoryModel",
                table: "TicketHistoryModel",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketHistoryModel_AspNetUsers_ChangedByUserId",
                table: "TicketHistoryModel",
                column: "ChangedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketHistoryModel_Tickets_TicketId",
                table: "TicketHistoryModel",
                column: "TicketId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
