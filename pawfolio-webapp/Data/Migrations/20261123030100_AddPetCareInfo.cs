using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using pawfolio_webapp.Data;

#nullable disable

namespace pawfolio_webapp.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261123030100_AddPetCareInfo")]
    public partial class AddPetCareInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Weight",
                table: "Pets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Medication",
                table: "Pets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Food",
                table: "Pets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Vaccination",
                table: "Pets",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Weight",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "Medication",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "Food",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "Vaccination",
                table: "Pets");
        }
    }
}
