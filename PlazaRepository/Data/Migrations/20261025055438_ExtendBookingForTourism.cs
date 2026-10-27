using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlazaRepository.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExtendBookingForTourism : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "RoomId",
                table: "Bookings",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "HotelId",
                table: "Bookings",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "BookingType",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EventId",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PackageId",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<DateTime>(
                name: "ServiceDate",
                table: "Bookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TransportId",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TripId",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BookingType_EventId",
                table: "Bookings",
                columns: new[] { "BookingType", "EventId" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BookingType_PackageId",
                table: "Bookings",
                columns: new[] { "BookingType", "PackageId" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BookingType_TransportId_ServiceDate",
                table: "Bookings",
                columns: new[] { "BookingType", "TransportId", "ServiceDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BookingType_TripId_ServiceDate",
                table: "Bookings",
                columns: new[] { "BookingType", "TripId", "ServiceDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_EventId",
                table: "Bookings",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_PackageId",
                table: "Bookings",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TransportId",
                table: "Bookings",
                column: "TransportId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TripId",
                table: "Bookings",
                column: "TripId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Bookings_Quantity",
                table: "Bookings",
                sql: "[Quantity] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Bookings_Target",
                table: "Bookings",
                sql: "([BookingType] = 0 AND [HotelId] IS NOT NULL AND [RoomId] IS NOT NULL AND [TripId] IS NULL AND [TransportId] IS NULL AND [EventId] IS NULL AND [PackageId] IS NULL) OR ([BookingType] = 1 AND [TripId] IS NOT NULL AND [HotelId] IS NULL AND [RoomId] IS NULL AND [TransportId] IS NULL AND [EventId] IS NULL AND [PackageId] IS NULL AND [ServiceDate] IS NOT NULL) OR ([BookingType] = 2 AND [TransportId] IS NOT NULL AND [HotelId] IS NULL AND [RoomId] IS NULL AND [TripId] IS NULL AND [EventId] IS NULL AND [PackageId] IS NULL AND [ServiceDate] IS NOT NULL) OR ([BookingType] = 3 AND [EventId] IS NOT NULL AND [HotelId] IS NULL AND [RoomId] IS NULL AND [TripId] IS NULL AND [TransportId] IS NULL AND [PackageId] IS NULL) OR ([BookingType] = 4 AND [PackageId] IS NOT NULL AND [TripId] IS NULL AND [TransportId] IS NULL AND [EventId] IS NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Events_EventId",
                table: "Bookings",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Packages_PackageId",
                table: "Bookings",
                column: "PackageId",
                principalTable: "Packages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Transports_TransportId",
                table: "Bookings",
                column: "TransportId",
                principalTable: "Transports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Trips_TripId",
                table: "Bookings",
                column: "TripId",
                principalTable: "Trips",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Events_EventId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Packages_PackageId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Transports_TransportId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Trips_TripId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_BookingType_EventId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_BookingType_PackageId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_BookingType_TransportId_ServiceDate",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_BookingType_TripId_ServiceDate",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_EventId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_PackageId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_TransportId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_TripId",
                table: "Bookings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Bookings_Quantity",
                table: "Bookings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Bookings_Target",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "BookingType",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "EventId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "PackageId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ServiceDate",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "TransportId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "TripId",
                table: "Bookings");

            migrationBuilder.AlterColumn<int>(
                name: "RoomId",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "HotelId",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
