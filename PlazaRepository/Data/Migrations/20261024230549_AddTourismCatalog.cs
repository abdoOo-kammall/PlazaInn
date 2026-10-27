using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlazaRepository.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTourismCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    LocationAr = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    EventDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TicketPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalTickets = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ImageIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.CheckConstraint("CK_Events_Dates", "[EndDate] IS NULL OR [EndDate] >= [EventDate]");
                    table.CheckConstraint("CK_Events_TicketPrice", "[TicketPrice] > 0");
                    table.CheckConstraint("CK_Events_TotalTickets", "[TotalTickets] > 0");
                });

            migrationBuilder.CreateTable(
                name: "Packages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    DurationDays = table.Column<int>(type: "int", nullable: false),
                    PackagePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MaxPackagesAvailable = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ImageIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HotelId = table.Column<int>(type: "int", nullable: true),
                    RoomId = table.Column<int>(type: "int", nullable: true),
                    NightsCount = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Packages", x => x.Id);
                    table.CheckConstraint("CK_Packages_Dates", "[EndDate] IS NULL OR ([StartDate] IS NOT NULL AND [EndDate] >= [StartDate])");
                    table.CheckConstraint("CK_Packages_DurationDays", "[DurationDays] > 0");
                    table.CheckConstraint("CK_Packages_HotelStayComplete", "([HotelId] IS NULL AND [RoomId] IS NULL AND [NightsCount] IS NULL) OR ([HotelId] IS NOT NULL AND [RoomId] IS NOT NULL AND [NightsCount] IS NOT NULL AND [NightsCount] > 0)");
                    table.CheckConstraint("CK_Packages_MaxPackagesAvailable", "[MaxPackagesAvailable] > 0");
                    table.CheckConstraint("CK_Packages_PackagePrice", "[PackagePrice] > 0");
                    table.ForeignKey(
                        name: "FK_Packages_Hotels_HotelId",
                        column: x => x.HotelId,
                        principalTable: "Hotels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Packages_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Transports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    VehicleType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VehicleTypeAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PickupLocation = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    PickupLocationAr = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    DropoffLocation = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    DropoffLocationAr = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    MaxPassengers = table.Column<int>(type: "int", nullable: false),
                    MaxLuggage = table.Column<int>(type: "int", nullable: false),
                    PricePerTrip = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AvailableVehicles = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ImageIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transports", x => x.Id);
                    table.CheckConstraint("CK_Transports_AvailableVehicles", "[AvailableVehicles] > 0");
                    table.CheckConstraint("CK_Transports_MaxLuggage", "[MaxLuggage] >= 0");
                    table.CheckConstraint("CK_Transports_MaxPassengers", "[MaxPassengers] > 0");
                    table.CheckConstraint("CK_Transports_PricePerTrip", "[PricePerTrip] > 0");
                });

            migrationBuilder.CreateTable(
                name: "Trips",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CityAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Itinerary = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    ItineraryAr = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    DurationDays = table.Column<int>(type: "int", nullable: false),
                    DurationHours = table.Column<int>(type: "int", nullable: false),
                    PricePerPerson = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PricePerChild = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MaxCapacity = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ImageIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trips", x => x.Id);
                    table.CheckConstraint("CK_Trips_Dates", "[EndDate] IS NULL OR ([StartDate] IS NOT NULL AND [EndDate] >= [StartDate])");
                    table.CheckConstraint("CK_Trips_Duration", "[DurationDays] > 0 OR [DurationHours] > 0");
                    table.CheckConstraint("CK_Trips_DurationDays", "[DurationDays] >= 0");
                    table.CheckConstraint("CK_Trips_DurationHours", "[DurationHours] >= 0");
                    table.CheckConstraint("CK_Trips_MaxCapacity", "[MaxCapacity] > 0");
                    table.CheckConstraint("CK_Trips_PricePerChild", "[PricePerChild] > 0");
                    table.CheckConstraint("CK_Trips_PricePerPerson", "[PricePerPerson] > 0");
                });

            migrationBuilder.CreateTable(
                name: "PackageEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackageId = table.Column<int>(type: "int", nullable: false),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackageEvents_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackageEvents_Packages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "Packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PackageTransports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackageId = table.Column<int>(type: "int", nullable: false),
                    TransportId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageTransports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackageTransports_Packages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "Packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PackageTransports_Transports_TransportId",
                        column: x => x.TransportId,
                        principalTable: "Transports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PackageTrips",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackageId = table.Column<int>(type: "int", nullable: false),
                    TripId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageTrips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackageTrips_Packages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "Packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PackageTrips_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_EventDate",
                table: "Events",
                column: "EventDate");

            migrationBuilder.CreateIndex(
                name: "IX_Events_EventDate_EndDate",
                table: "Events",
                columns: new[] { "EventDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Events_IsActive",
                table: "Events",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PackageEvents_EventId",
                table: "PackageEvents",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageEvents_PackageId_EventId",
                table: "PackageEvents",
                columns: new[] { "PackageId", "EventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Packages_HotelId",
                table: "Packages",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_Packages_IsActive",
                table: "Packages",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Packages_RoomId",
                table: "Packages",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Packages_StartDate_EndDate",
                table: "Packages",
                columns: new[] { "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PackageTransports_PackageId_TransportId",
                table: "PackageTransports",
                columns: new[] { "PackageId", "TransportId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PackageTransports_TransportId",
                table: "PackageTransports",
                column: "TransportId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageTrips_PackageId_TripId",
                table: "PackageTrips",
                columns: new[] { "PackageId", "TripId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PackageTrips_TripId",
                table: "PackageTrips",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_Transports_IsActive",
                table: "Transports",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Transports_VehicleType",
                table: "Transports",
                column: "VehicleType");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_City",
                table: "Trips",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_IsActive",
                table: "Trips",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_StartDate_EndDate",
                table: "Trips",
                columns: new[] { "StartDate", "EndDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PackageEvents");

            migrationBuilder.DropTable(
                name: "PackageTransports");

            migrationBuilder.DropTable(
                name: "PackageTrips");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Transports");

            migrationBuilder.DropTable(
                name: "Packages");

            migrationBuilder.DropTable(
                name: "Trips");
        }
    }
}
