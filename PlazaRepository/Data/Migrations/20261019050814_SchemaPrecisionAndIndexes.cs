using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlazaRepository.Data.Migrations
{
    /// <inheritdoc />
    public partial class SchemaPrecisionAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_HotelId",
                table: "Bookings");

            migrationBuilder.AlterColumn<string>(
                name: "Hash",
                table: "Images",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentReference",
                table: "Bookings",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerEmail",
                table: "Bookings",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Images_Hash_Type",
                table: "Images",
                columns: new[] { "Hash", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CustomerEmail",
                table: "Bookings",
                column: "CustomerEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_HotelId_CheckIn_CheckOut",
                table: "Bookings",
                columns: new[] { "HotelId", "CheckIn", "CheckOut" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_PaymentReference",
                table: "Bookings",
                column: "PaymentReference");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Images_Hash_Type",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_CustomerEmail",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_HotelId_CheckIn_CheckOut",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_PaymentReference",
                table: "Bookings");

            migrationBuilder.AlterColumn<string>(
                name: "Hash",
                table: "Images",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentReference",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerEmail",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_HotelId",
                table: "Bookings",
                column: "HotelId");
        }
    }
}
