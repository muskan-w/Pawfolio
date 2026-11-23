using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pawfolio_webapp.Data.Migrations
{
    /// <inheritdoc />
    public partial class MoveVeterinaryInfoToVetInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VetInfos",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LicenseNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ClinicName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ClinicAddress = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ClinicContact = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ClinicianPosition = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApprovalStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VetInfos", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_VetInfos_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                INSERT INTO VetInfos (UserId, LicenseNumber, ClinicName, ClinicAddress, ClinicContact, ClinicianPosition, ApprovalStatus)
                SELECT Id,
                       COALESCE(LicenseNumber, ''),
                       COALESCE(ClinicName, ''),
                       COALESCE(ClinicAddress, ''),
                       COALESCE(ClinicContact, ''),
                       COALESCE(ClinicianPosition, ''),
                       COALESCE(ApprovalStatus, 'Approved')
                FROM AspNetUsers
                WHERE Role = 'VeterinaryProfessional';
                """);

            migrationBuilder.DropColumn(name: "ApprovalStatus", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "ClinicAddress", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "ClinicContact", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "ClinicName", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "ClinicianPosition", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "LicenseNumber", table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovalStatus",
                table: "AspNetUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ClinicAddress",
                table: "AspNetUsers",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClinicContact",
                table: "AspNetUsers",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClinicName",
                table: "AspNetUsers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClinicianPosition",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LicenseNumber",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE users
                SET ApprovalStatus = vetInfo.ApprovalStatus,
                    ClinicAddress = vetInfo.ClinicAddress,
                    ClinicContact = vetInfo.ClinicContact,
                    ClinicName = vetInfo.ClinicName,
                    ClinicianPosition = vetInfo.ClinicianPosition,
                    LicenseNumber = vetInfo.LicenseNumber
                FROM AspNetUsers users
                INNER JOIN VetInfos vetInfo ON vetInfo.UserId = users.Id;
                """);

            migrationBuilder.DropTable(name: "VetInfos");
        }
    }
}
