using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlazaRepository.Data.Migrations
{
    /// <inheritdoc />
    public partial class aboutUsEdition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageIds",
                table: "AboutUs",
                newName: "Square2ImageId");

            migrationBuilder.AddColumn<string>(
                name: "CircleImageId",
                table: "AboutUs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Square1ImageId",
                table: "AboutUs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CircleImageId",
                table: "AboutUs");

            migrationBuilder.DropColumn(
                name: "Square1ImageId",
                table: "AboutUs");

            migrationBuilder.RenameColumn(
                name: "Square2ImageId",
                table: "AboutUs",
                newName: "ImageIds");
        }
    }
}
