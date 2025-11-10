using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlazaRepository.Data.Migrations
{
    /// <inheritdoc />
    public partial class addionatioalHotelP03 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NumOfSuites",
                table: "Hotels",
                newName: "NumOfAvailableSuitesToReserve");

            migrationBuilder.RenameColumn(
                name: "NumOfRooms",
                table: "Hotels",
                newName: "NumOfAvailableRoomsToReserve");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NumOfAvailableSuitesToReserve",
                table: "Hotels",
                newName: "NumOfSuites");

            migrationBuilder.RenameColumn(
                name: "NumOfAvailableRoomsToReserve",
                table: "Hotels",
                newName: "NumOfRooms");
        }
    }
}
