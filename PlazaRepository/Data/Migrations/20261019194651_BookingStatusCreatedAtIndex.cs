using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlazaRepository.Data.Migrations
{
    /// <inheritdoc />
    public partial class BookingStatusCreatedAtIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Bookings_Status_CreatedAt",
                table: "Bookings",
                columns: new[] { "Status", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_Status_CreatedAt",
                table: "Bookings");
        }
    }
}
