using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTracingContextToAccountInbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TraceParent",
                table: "InboxMessages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TraceState",
                table: "InboxMessages",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TraceParent",
                table: "InboxMessages");

            migrationBuilder.DropColumn(
                name: "TraceState",
                table: "InboxMessages");
        }
    }
}
