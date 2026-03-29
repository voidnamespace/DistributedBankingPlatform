using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankCardService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToCardNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Enforce card number uniqueness at the database level (source of truth)
            migrationBuilder.CreateIndex(
                name: "UX_BankCards_CardNumber",
                table: "BankCards",
                column: "CardNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_BankCards_CardNumber",
                table: "BankCards");
        }
    }
}
