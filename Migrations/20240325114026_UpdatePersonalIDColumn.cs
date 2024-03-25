using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalWebsiteApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePersonalIDColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "PERSONALID",
                schema: "dbo",
                table: "HOSP_USERS",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AlterColumn<int>(
                name: "PERSONALID",
                schema: "dbo",
                table: "HOSP_USERS",
                type: "int",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
