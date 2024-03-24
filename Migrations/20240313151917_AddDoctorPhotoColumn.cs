using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalWebsiteApi.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorPhotoColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(name: "PHOTO", table: "HOSP_USERS", nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "PHOTO", table: "HOSP_USERS");
        }
    }
}
