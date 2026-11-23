using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using pawfolio_webapp.Data;

#nullable disable

namespace pawfolio_webapp.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261123030000_AddPetProfilePicture")]
    public partial class AddPetProfilePicture : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageContentType",
                table: "Pets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "Pets",
                type: "varbinary(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageContentType",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "Pets");
        }
    }
}
