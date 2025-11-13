using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlazaRepository.Data.Migrations
{
    /// <inheritdoc />
    public partial class addArDescriptionAndTictok : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CafeDescriptionAr",
                table: "InsightHotel",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RestaurantDescriptionAr",
                table: "InsightHotel",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tictok",
                table: "Hotels",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CafeDescriptionAr",
                table: "InsightHotel");

            migrationBuilder.DropColumn(
                name: "RestaurantDescriptionAr",
                table: "InsightHotel");

            migrationBuilder.DropColumn(
                name: "Tictok",
                table: "Hotels");
        }
    }
}
