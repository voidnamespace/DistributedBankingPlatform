using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransactionService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RevomedRoutingKeyFromOutboxInbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoutingKey",
                table: "OutboxMessages");

            migrationBuilder.CreateTable(
                name: "InboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    Processed = table.Column<bool>(type: "boolean", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    Error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxMessages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessages_ProcessedAt",
                table: "InboxMessages",
                column: "ProcessedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InboxMessages");

            migrationBuilder.AddColumn<string>(
                name: "RoutingKey",
                table: "OutboxMessages",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");
        }
    }
}
