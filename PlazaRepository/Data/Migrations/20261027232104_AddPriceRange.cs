using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlazaRepository.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceRange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PriceRange",
                table: "Trips",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PriceRange",
                table: "Transports",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PriceRange",
                table: "Packages",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PriceRange",
                table: "Hotels",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PriceRange",
                table: "Events",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PriceRange",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "PriceRange",
                table: "Transports");

            migrationBuilder.DropColumn(
                name: "PriceRange",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "PriceRange",
                table: "Hotels");

            migrationBuilder.DropColumn(
                name: "PriceRange",
                table: "Events");
        }
    }
}
