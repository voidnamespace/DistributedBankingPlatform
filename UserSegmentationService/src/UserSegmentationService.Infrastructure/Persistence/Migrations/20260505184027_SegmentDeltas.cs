using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserSegmentationService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SegmentDeltas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SegmentDeltas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SegmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    AddedUserIds = table.Column<Guid[]>(type: "uuid[]", nullable: false),
                    RemovedUserIds = table.Column<Guid[]>(type: "uuid[]", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegmentDeltas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SegmentDeltas_Segments_SegmentId",
                        column: x => x.SegmentId,
                        principalTable: "Segments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SegmentDeltas_CreatedAt",
                table: "SegmentDeltas",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SegmentDeltas_SegmentId",
                table: "SegmentDeltas",
                column: "SegmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SegmentDeltas");
        }
    }
}
