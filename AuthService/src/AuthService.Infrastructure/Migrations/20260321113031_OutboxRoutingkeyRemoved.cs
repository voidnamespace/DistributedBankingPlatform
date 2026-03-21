using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OutboxRoutingkeyRemoved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoutingKey",
                table: "OutboxMessages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
