using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tasker.BoardWrite.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixBoardWriteModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "BoardId",
                table: "labels",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_columns_BoardId_Order",
                table: "columns",
                columns: new[] { "BoardId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_cards_BoardId_ColumnId_Order",
                table: "cards",
                columns: new[] { "BoardId", "ColumnId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_boards_OwnerUserId",
                table: "boards",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_board_members_BoardId_UserId",
                table: "board_members",
                columns: new[] { "BoardId", "UserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_columns_BoardId_Order",
                table: "columns");

            migrationBuilder.DropIndex(
                name: "IX_cards_BoardId_ColumnId_Order",
                table: "cards");

            migrationBuilder.DropIndex(
                name: "IX_boards_OwnerUserId",
                table: "boards");

            migrationBuilder.DropIndex(
                name: "IX_board_members_BoardId_UserId",
                table: "board_members");

            migrationBuilder.AlterColumn<Guid>(
                name: "BoardId",
                table: "labels",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");
        }
    }
}
