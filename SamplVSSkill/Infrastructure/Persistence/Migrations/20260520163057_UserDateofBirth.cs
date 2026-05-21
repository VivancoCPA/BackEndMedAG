using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SamplVSSkill.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UserDateofBirth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "date_of_birth",
                table: "AspNetUsers",
                newName: "DateOfBirth");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DateOfBirth",
                table: "AspNetUsers",
                newName: "date_of_birth");
        }
    }
}
