using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParksApi.Migrations
{
    /// <inheritdoc />
    public partial class AddDistinguishNameFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Parks",
                newName: "ParkName");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Centers",
                newName: "CenterName");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "AspNetUsers",
                newName: "GivenName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ParkName",
                table: "Parks",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "CenterName",
                table: "Centers",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "GivenName",
                table: "AspNetUsers",
                newName: "Name");
        }
    }
}
