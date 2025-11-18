using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlazaRepository.Data.Migrations
{
    /// <inheritdoc />
    public partial class MenuAddtion02 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EntityType",
                table: "Images");

            migrationBuilder.RenameColumn(
                name: "RestaurantMenuPdf",
                table: "InsightHotel",
                newName: "RestaurantMenuPdfIds");

            migrationBuilder.RenameColumn(
                name: "RestaurantImages",
                table: "InsightHotel",
                newName: "RestaurantImageIds");

            migrationBuilder.RenameColumn(
                name: "CafeMenuPdf",
                table: "InsightHotel",
                newName: "CafeMenuPdfIds");

            migrationBuilder.RenameColumn(
                name: "CafeImages",
                table: "InsightHotel",
                newName: "CafeImageIds");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Images",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Images");

            migrationBuilder.RenameColumn(
                name: "RestaurantMenuPdfIds",
                table: "InsightHotel",
                newName: "RestaurantMenuPdf");

            migrationBuilder.RenameColumn(
                name: "RestaurantImageIds",
                table: "InsightHotel",
                newName: "RestaurantImages");

            migrationBuilder.RenameColumn(
                name: "CafeMenuPdfIds",
                table: "InsightHotel",
                newName: "CafeMenuPdf");

            migrationBuilder.RenameColumn(
                name: "CafeImageIds",
                table: "InsightHotel",
                newName: "CafeImages");

            migrationBuilder.AddColumn<string>(
                name: "EntityType",
                table: "Images",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
