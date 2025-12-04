using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tasker.BoardWrite.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCardAssignees : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "assignee_user_ids",
                table: "cards",
                type: "longtext",
                nullable: false,
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "assignee_user_ids",
                table: "cards");
        }
    }
}
